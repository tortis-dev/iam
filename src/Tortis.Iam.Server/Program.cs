using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Tortis.Iam.Server.Components;
using Tortis.Iam.Server.Components.Account;
using Tortis.Iam.Server.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString,
        sql => sql.MigrationsHistoryTable("iam_schema_migrations_history", "iam"));

    // options.UseSqlite(connectionString,
    //     sql => sql.MigrationsHistoryTable("iam_schema_migrations_history"));
    
    options.UseOpenIddict<Guid>();
    
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//OIDC
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options
            .UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>()
            .ReplaceDefaultEntities<Guid>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("connect/token");
        options.SetAuthorizationEndpointUris("connect/authorize");
        options.SetConfigurationEndpointUris(".well-known/openid-configuration");
        options.AllowClientCredentialsFlow();
        options.AllowAuthorizationCodeFlow();
        options.AllowHybridFlow();
        options.AllowRefreshTokenFlow();

        options.AddDevelopmentSigningCertificate();
        options.DisableAccessTokenEncryption();
        //Data Encryption
        options.AddDevelopmentEncryptionCertificate();

        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough();
    });

//Identity
builder.Services
    .AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>()
    .AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddHostedService<SetupDefaultAdmin>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
app.Run();
