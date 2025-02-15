using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using OpenIddict.Abstractions;
using Quartz;
using Tortis.Iam.Server.Components;
using Tortis.Iam.Server.Components.Account;
using Tortis.Iam.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.ConfigureEndpointDefaults(endpoint => endpoint.UseHttps());
});

// Configure MVC
builder.Services.AddControllers();

// Configure Blazor
builder.Services
    .AddFluentUIComponents()
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// Authentication/Authorization (for IAM itself)
builder.Services
    .AddCascadingAuthenticationState()
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<IamDbContext>(options =>
{
    // TODO: Get provider and schema from config
    options.UseSqlServer(connectionString,
        sql => sql.MigrationsHistoryTable("iam_schema_migrations_history", "iam"));
    
    // Use OpenIdDict entities with Guid ID type
    options.UseOpenIddict<Guid>();
    
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// OpenIdDict uses Quartz to schedule background jobs for cleaning up token caches.
builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore(); //TODO: Make this configurable with the ability to use the database.
}).AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

// OIDC
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options
            .UseEntityFrameworkCore()
            .UseDbContext<IamDbContext>()
            .ReplaceDefaultEntities<Guid>();
        options.UseQuartz();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("connect/token");
        options.SetAuthorizationEndpointUris("connect/authorize");
        options.SetConfigurationEndpointUris(".well-known/openid-configuration");
        options.AllowClientCredentialsFlow();
        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        options.AllowHybridFlow();
        options.AllowRefreshTokenFlow();
        
        options.DisableAccessTokenEncryption(); // TODO: From Config

        if (builder.Environment.IsDevelopment())
        {
            options.AddDevelopmentEncryptionCertificate(); //Data Encryption
            options.AddDevelopmentSigningCertificate();
        }

        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough();
        
        // Need to register addition scopes supported. By default, openid and offline_access are added.
        // AspNet Core apps request openid profile by default.
        // It appears custom scopes do not need to be added?
        options.RegisterScopes(OpenIddictConstants.Scopes.Profile);
        options.RegisterScopes(OpenIddictConstants.Scopes.Email);
        options.RegisterScopes(OpenIddictConstants.Scopes.Roles);
    });

// Identity
builder.Services
    .AddScoped<IdentityUserAccessor>()
    .AddScoped<IdentityRedirectManager>()
    .AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>()
    .AddSingleton<IEmailSender<IamUser>, IdentityNoOpEmailSender>()
    .AddIdentityCore<IamUser>(options =>
    {
        //TODO: Default password policy to current NIST/NSA recommendations
        //TODO: from config
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<IamDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Application services
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<SetupDefaultAdmin>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0#usestatuscodepageswithredirects
//Keeps original URL in address bar, does not render layout
//app.UseStatusCodePagesWithReExecute("/not-found");
//Renders layout but changes url in address bar;
app.UseStatusCodePagesWithRedirects("/not-found/{0}");
app.UseStaticFiles();
app.UseAntiforgery();
app.MapHealthChecks("/health");
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity `/Account` Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
