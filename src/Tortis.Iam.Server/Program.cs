using System.Reflection;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using OpenIddict.Abstractions;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Quartz;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

using Tortis.Iam.Server;
using Tortis.Iam.Server.Components;
using Tortis.Iam.Server.Components.Account;
using Tortis.Iam.Server.Components.Applications;
using Tortis.Iam.Server.Components.OpenIdConnect;
using Tortis.Iam.Server.Components.Resources;
using Tortis.Iam.Server.Components.Roles;
using Tortis.Iam.Server.Components.Users;
using Tortis.Iam.Server.Data;

const string consoleLogTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties} {NewLine}{Exception}";

// A bare minimum bootstrap logger to capture any exceptions thrown during startup.
Log.Logger = new LoggerConfiguration().WriteTo.Console(outputTemplate: consoleLogTemplate).CreateBootstrapLogger();

try
{
    Log.Information("Starting up...");
    var instanceId = Guid.NewGuid().ToString();
    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddEnvironmentVariables("TORTIS__IAM__") // We want to use a custom prefix for environment variables.
        .AddCommandLine(args); // We still want command line arguments to override environment variables.

    // Configure Kestrel
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader =
            false; // Remove the default "Server" header so attackers can't identify the server as Kestrel.
        options.ConfigureEndpointDefaults(endpoint => endpoint.UseHttps());
    });

    // Logging, monitoring, and telemetry
    builder.Logging.ClearProviders();
    builder.Services.AddSerilog(loggerConfiguration => 
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
        .Enrich.WithProperty("Version", version)
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
        .Enrich.FromLogContext()
        .Enrich.WithSpan()
        .WriteTo.Async(l => l.Console(outputTemplate: consoleLogTemplate))
        .WriteTo.Async(l => l.OpenTelemetry(resourceAttributes: new Dictionary<string, object>()
        {
            { "service.name", builder.Environment.ApplicationName },
            { "service.version", version },
            { "service.instance.id", instanceId }
        })));

    builder.Services.AddHealthChecks();

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource =>
            resource
                .AddService(
                    serviceName: builder.Environment.ApplicationName,
                    serviceVersion: version,
                    serviceInstanceId: instanceId)
        )
        .WithMetrics(metrics =>
            metrics
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation())
        .WithTracing(tracing =>
            tracing
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation());



    // Configure MVC
    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // Configure Blazor
    builder.Services
        .AddFluentUIComponents()
        .AddRazorComponents()
        .AddInteractiveServerComponents();
    builder.Services.AddDataGridEntityFrameworkAdapter();

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
                sql => sql.MigrationsHistoryTable(IamDbContext.HISTORY_TABLE_NAME, databaseSchema));
        else if (databaseProvider == "Sqlite")
            options.UseSqlite(
                $"Filename=./Data/{databaseSchema}.db",
                sqlite => sqlite.MigrationsHistoryTable(IamDbContext.HISTORY_TABLE_NAME));

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
        .AddScoped<IamResourceManager>()
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
            var settings = builder.Configuration.GetSection("OpenIdConnect").Get<OpenIdConnectSettings>() ??
                           new OpenIdConnectSettings();
            builder.Services.AddSingleton(settings);

            options.SetConfigurationEndpointUris(".well-known/openid-configuration");
            options.SetTokenEndpointUris(TortisOpenIdConstants.TOKEN_ENDPOINT);
            options.SetAuthorizationEndpointUris(TortisOpenIdConstants.AUTHORIZATION_ENDPOINT);
            options.SetUserInfoEndpointUris(TortisOpenIdConstants.USERINFO_ENDPOINT);

            if (settings.EnableClientCredentialsFlow) options.AllowClientCredentialsFlow();
            if (settings.EnableAuthorizationCodeFlow) options.AllowAuthorizationCodeFlow();
            if (settings.RequirePkceGlobally) options.RequireProofKeyForCodeExchange();
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
        .AddRoles<IamRole>()
        .AddEntityFrameworkStores<IamDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    // Application services
    builder.Services.AddHostedService<SetupDefaultAdmin>();

    var app = builder.Build();

    // var quartz = Quarts
    // if (!quartz.Clustered)
    // {
    //     app.Logger.LogWarning("Quartz is not clustered. This is not recommended for production.");
    // }

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
        ExceptionHandlingPath = "/error",
        ExceptionHandler = context =>
        {
            if (context.Request.Path.StartsWithSegments("connect")
                || context.Request.Path.StartsWithSegments("api"))
            {
                context.Response.StatusCode = 500;
                return context.Response.WriteAsync("An error occurred while processing your request.");
            }

            return Task.CompletedTask;
        }
    });

    app.MapHealthChecks("/health");
    app.MapControllers();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode().RequireAuthorization();

    // Add additional endpoints required by the Identity `/Account` Razor components.
    app.MapAdditionalIdentityEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}