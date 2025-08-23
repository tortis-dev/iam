using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using OpenIddict.Abstractions;
using Quartz;

using Tortis.Iam.Server;
using Tortis.Iam.Server.Components;
using Tortis.Iam.Server.Components.Account;
using Tortis.Iam.Server.Components.Applications;
using Tortis.Iam.Server.Components.OpenIdConnect;
using Tortis.Iam.Server.Components.Users;
using Tortis.Iam.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false; // Remove the default "Server" header so attackers can't identify the server as Kestrel.
    options.ConfigureEndpointDefaults(endpoint => endpoint.UseHttps());
});

// Configure MVC
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure Blazor
builder.Services
    .AddFluentUIComponents()
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// Authentication/Authorization
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

var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
var databaseSchema = builder.Configuration.GetValue<string>("DatabaseSchema") ?? "iam";

builder.Services.AddDbContext<IamDbContext>(options =>
{
    if (databaseProvider == "SqlServer")
        options.UseSqlServer(connectionString,
            sql => sql.MigrationsHistoryTable(IamDbContext.HistoryTableName, databaseSchema));
    else if (databaseProvider == "Sqlite")
        options.UseSqlite(
            $"Filename=./Data/{databaseSchema}.db", 
            sqlite => sqlite.MigrationsHistoryTable(IamDbContext.HistoryTableName));
    
    // Use OpenIdDict entities with Guid ID type
    options.UseOpenIddict<Guid>();
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// OpenIdDict uses Quartz to schedule background jobs for cleaning up token caches.
builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
    // options.UsePersistentStore(store =>
    // {
    //     store.UseSystemTextJsonSerializer();
    //     store.UseClustering();
    //     if (databaseProvider == "SqlServer") store.UseSqlServer(sql =>
    //     {
    //         sql.ConnectionString = connectionString;
    //         sql.TablePrefix = $"[{databaseSchema}].qrtz_";
    //     });
    // });
}).AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

// OIDC
builder.Services
    .AddScoped<IOpenIddictApplicationManager, IamApplicationManager>()
    .AddOpenIddict()
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
        var settings = builder.Configuration.GetSection("OpenIdConnect").Get<OpenIdConnectSettings>() ?? new OpenIdConnectSettings();
        builder.Services.AddSingleton(settings);
        
        options.SetConfigurationEndpointUris(".well-known/openid-configuration");
        options.SetTokenEndpointUris(TortisOpenIdConstants.TOKEN_ENDPOINT);
        options.SetAuthorizationEndpointUris(TortisOpenIdConstants.AUTHORIZATION_ENDPOINT);
        options.SetUserInfoEndpointUris(TortisOpenIdConstants.USERINFO_ENDPOINT);
        
        if (settings.EnableClientCredentialsFlow) options.AllowClientCredentialsFlow();
        if (settings.EnableAuthorizationCodeFlow) options.AllowAuthorizationCodeFlow();
        if (settings.RequirePkce) options.RequireProofKeyForCodeExchange();
        if (settings.EnableHybridFlow) options.AllowHybridFlow();
        if (settings.EnableRefreshTokenFlow) options.AllowRefreshTokenFlow();
        
        if (!settings.EnableAccessTokenEncryption)
            options.DisableAccessTokenEncryption();
        
        if (builder.Environment.IsDevelopment())
        {
            options.AddDevelopmentEncryptionCertificate(); //Data Encryption
            options.AddDevelopmentSigningCertificate();
        }
        
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough();
        
        // Need to register additional scopes supported. By default, openid and offline_access are added.
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
    // The default HTTP Strict Transport Security (HSTS) value is 30 days.
    // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = context =>
    {
        context.Response.StatusCode = 500;
        return context.Response.WriteAsync("An error occurred while processing your request.");
    }
});

app.MapHealthChecks("/health");
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().RequireAuthorization();

// Add additional endpoints required by the Identity `/Account` Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
