using System.Reflection;
using System.Resources;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

const string CONSOLE_LOG_TEMPLATE = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties} {NewLine}{Exception}";
const string DEFAULT_SQLITE_DATABASE_PATH = "./Data/iam.db";
const string DEFAULT_SQLITE_QUARTZ_DATABASE_PATH = "./Data/iam_quartz.db";

// A bare minimum bootstrap logger to capture any exceptions thrown during startup.
var logger = new LoggerConfiguration().WriteTo.Console(outputTemplate: CONSOLE_LOG_TEMPLATE).CreateBootstrapLogger();

try
{
    logger.Information("Starting up...");
    var instanceId = Guid.NewGuid().ToString();
    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddEnvironmentVariables("TORTIS__IAM__") // We want to use a custom prefix for environment variables.
        .AddCommandLine(args) // We still want command line arguments to override environment variables.
        .AddTestConfiguration(); // Unit tests can use this to flow state to the main program and change configuration.

    // Configure Kestrel
    builder.WebHost.ConfigureKestrel(options =>
    {
        // Remove the default "Server" header so attackers can't identify the server as Kestrel.
        options.AddServerHeader = false; 
    });

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromSeconds(10);
        });
    }
    else
    {
        builder.Services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(60);
        });
    }

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
            .WriteTo.Async(l => l.Console(outputTemplate: CONSOLE_LOG_TEMPLATE))
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
                .AddOtlpExporter()
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation())
        .WithTracing(tracing =>
            tracing
                .AddOtlpExporter()
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                }));

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
    builder.AddAuthorization()
        .Services
        .AddCascadingAuthenticationState()
        .AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

    // Database
    // TODO: Make database settings a strong type config.
    var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
    string appConnectionString = string.Empty;
    if (string.Equals(databaseProvider, "sqlite", StringComparison.OrdinalIgnoreCase))
    {
        appConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                              $"Filename={DEFAULT_SQLITE_DATABASE_PATH}";
    }
    else
    {
        appConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    var databaseSchema = builder.Configuration.GetValue<string>("DatabaseSchema") ?? "iam";

    builder.Services.AddDbContext<IamDbContext>(options =>
    {
        if (string.Equals(databaseProvider, "inmemory", StringComparison.OrdinalIgnoreCase))
        {
            var connection = new SqliteConnection($"Filename=:memory:");
            options.UseSqlite(connection);
        }
        else if (databaseProvider == "Sqlite")
        {
            options.UseSqlite(appConnectionString,
                sqlite => sqlite.MigrationsHistoryTable(IamDbContext.HISTORY_TABLE_NAME));
        }
        else
        {
            if (databaseProvider == "SqlServer")
                options.UseSqlServer(appConnectionString,
                    sql => sql.MigrationsHistoryTable(IamDbContext.HISTORY_TABLE_NAME, databaseSchema));
        }

        // Use OpenIdDict entities with Guid ID type
        options.UseOpenIddict<Guid>();
    });

    // OpenIdDict uses Quartz to schedule background jobs for cleaning up token caches.
    builder.Services.AddQuartz(options =>
    {
        options.UseSimpleTypeLoader();
        if (string.Equals(databaseProvider, "inmemory", StringComparison.OrdinalIgnoreCase))
        {
            options.UseInMemoryStore();
        }
        else
        {
            options.UsePersistentStore(store =>
            {
                store.UseSystemTextJsonSerializer();
                if (string.Equals(databaseProvider, "sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    store.UseSQLite($"Filename={DEFAULT_SQLITE_QUARTZ_DATABASE_PATH}");
                }
                else if (string.Equals(databaseProvider, "sqlserver", StringComparison.OrdinalIgnoreCase))
                {
                    store.UseClustering();
                    store.UseSqlServer(sql =>
                    {

                        sql.ConnectionString = appConnectionString;
                        sql.TablePrefix = $"{databaseSchema}.qrtz_";
                    });
                }
            });
        }
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
            options.SignIn.RequireConfirmedAccount =
                builder.Configuration.GetValue<bool>("SignIn.RequireConfirmedAccount");
        })
        .AddRoles<IamRole>()
        .AddEntityFrameworkStores<IamDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    // Application services
    builder.Services
        .AddHostedService<SetupDefaultAdmin>()
        .Replace(ServiceDescriptor.Scoped<IUserClaimsPrincipalFactory<IamUser>, IamUserClaimsPrincipalFactory>())
        .AddScoped<IamUserManager>()
        .AddScoped<IamRoleManager>();

    var app = builder.Build();

    // When using Sqlite (file or :memory:), we want to automatically create the iam databases.
    // For other databases, we assume the database is created outside of the application.
    if (string.Equals(databaseProvider, "sqlite", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(databaseProvider, "inmemory", StringComparison.OrdinalIgnoreCase))

    {
        // Ensure the application database is created. This will only create a new file. It does not perform a migration.
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IamDbContext>();
            dbContext.Database.EnsureCreated();
        }
    }
    if (string.Equals(databaseProvider, "sqlite", StringComparison.OrdinalIgnoreCase))
    {
        // Ensure the Quartz database is created. This will only create a new file. It does not perform a migration.
        if (!File.Exists(DEFAULT_SQLITE_QUARTZ_DATABASE_PATH))
        {
            Log.Information("Initializing Quartz database...");
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("qrtz_sqlite_schema.sql", StringComparison.OrdinalIgnoreCase));
            if (resourceName is null)
                throw new InvalidOperationException("Embedded SQL resource not found.");
        
            string sql;
            using (var stream = assembly.GetManifestResourceStream(resourceName)!)
            using (var reader = new StreamReader(stream))
            {
                sql = reader.ReadToEnd();
            }
        
            using (var cn = new SqliteConnection($"Filename={DEFAULT_SQLITE_QUARTZ_DATABASE_PATH}"))
            {
                cn.Open();
                using (var sqliteCommand = new SqliteCommand(sql, cn))
                    sqliteCommand.ExecuteNonQuery();
            }
            Log.Information("Quartz database initialization complete.");
        }
    }

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
    
    // We want health checks available on http, but everything else should be https.
    app.Use(async (context, next) =>
    {
        // Skip HTTPS redirect for /health
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await next();
            return;
        }
        
        // If HTTP, redirect to HTTPS
        if (!context.Request.IsHttps)
        {
            var withHttps = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            context.Response.Redirect(withHttps, permanent: false);
            return;
        }
        
        await next();
    });
    
    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        // The default HTTP Strict Transport Security (HSTS) value is 30 days.
        // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    //app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();
    
    app.MapHealthChecks("/health");
    app.MapControllers();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode().RequireAuthorization();
    app.MapAdditionalIdentityEndpoints();

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    // Flushes any log messages and closes the logger.
    logger.Dispose();
}

/// <summary>
/// https://github.com/dotnet/aspnetcore/issues/37680#issuecomment-1331559463
/// </summary>
internal static class TestConfiguration
{
    // This async local is set in from tests and it flows to main
    static readonly AsyncLocal<Action<IConfigurationBuilder>?> _current = new();

    /// <summary>
    /// Adds the current test configuration to the application in the "right" place
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder</param>
    /// <returns>The modified <see cref="IConfigurationBuilder"/></returns>
    public static IConfigurationBuilder AddTestConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        if (_current.Value is { } configure)
        {
            configure(configurationBuilder);
        }

        return configurationBuilder;
    }

    /// <summary>
    /// Unit tests can use this to flow state to the main program and change configuration
    /// </summary>
    /// <param name="action"></param>
    public static void Create(Action<IConfigurationBuilder> action)
    {
        _current.Value = action;
    }
}