using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

using Tortis.Iam.Server.Components.Resources;
using Tortis.Iam.Server.Components.Roles;
using Tortis.Iam.Server.Components.Users;

namespace Tortis.Iam.Server.Data;

sealed class SetupDefaultAdmin : BackgroundService
{
    readonly IServiceProvider _container;
    readonly ILogger<SetupDefaultAdmin> _logger;
    private readonly IHostEnvironment _env;

    public SetupDefaultAdmin(IServiceProvider container, ILogger<SetupDefaultAdmin> logger, IHostEnvironment env)
    {
        _container = container;
        _logger = logger;
        _env = env;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _container.CreateAsyncScope();
        
        await scope.ServiceProvider.GetRequiredService<IamDbContext>().Database.EnsureCreatedAsync(stoppingToken);
        
        try
        {
            await CreateAdministratorRoleAsync(scope.ServiceProvider);
            await CreateAdminUserAsync(scope.ServiceProvider, stoppingToken);

            if (_env.IsDevelopment())
            {
                await CreateTestResourceAsync(scope.ServiceProvider, stoppingToken);
                await CreateTestClientAsync(scope.ServiceProvider, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            // We don't want to crash the application, but we do want to log a critical message.
            // A generic exception is being caught here because database errors are raised as their respective
            // platform exception--e.g. Microsoft.Data.SqlClient.SqlException.
            
            _logger.LogCritical(ex, 
                "An exception occurred while setting up the default administrator account. " +
                "Tortis IAM will continue to run, but you may not be able to log in as the default administrator. " +
                "Please ensure that the database is available and that the connection string is correct.");
        }
    }

    async Task CreateAdministratorRoleAsync(IServiceProvider container)
    {
        var roleManager = container.GetRequiredService<RoleManager<IamRole>>();
        await roleManager.CreateAsync(new IamRole("Administrator")
        {
            CreatedBy = "Installer"
        });
    }

    async Task CreateAdminUserAsync(IServiceProvider container, CancellationToken stoppingToken)
    {
        const string username = "Admin";
        const string defaultPassword = "Admin1234!";
        
        var userManager = container.GetRequiredService<UserManager<IamUser>>();
        var store = container.GetRequiredService<IUserStore<IamUser>>();
        
        var normalizedUsername = userManager.NormalizeName(username);
        var admin = await store.FindByNameAsync(normalizedUsername, stoppingToken);

        if (admin is null)
        {
            admin = new IamUser
            {
                Id = Ulid.NewUlid().ToGuid(),
                UserName = username,
                EmailConfirmed = true,
                CreatedOn = DateTimeOffset.Now,
                CreatedBy = "Installer"
            };
            
            var result = await userManager.CreateAsync(admin, defaultPassword);

            if (!result.Succeeded)
            {
                _logger.LogCritical(result.ToString());
                Environment.Exit(1);
            }

            await userManager.AddToRoleAsync(admin, "Administrator");
        }

#if DEBUG
        var numUsers = userManager.Users.Count();
        if (numUsers < 50)
            for (int i = 1; i < 50; i++)
            {
                var result = await userManager.CreateAsync(new IamUser
                {
                    Id = Ulid.NewUlid().ToGuid(),
                    UserName = $"TestUser{i}@acme.com",
                    Email = $"TestUser{i}@acme.com",
                    EmailConfirmed = true,
                    PhoneNumber = $"555-555-55{i,2:00}",
                    CreatedOn = DateTimeOffset.Now,
                    CreatedBy = "Installer"
                }, defaultPassword);
            }
#endif
    }

    async Task CreateTestResourceAsync(IServiceProvider container, CancellationToken stoppingToken)
    {
        var applicationManager = container.GetRequiredService<IOpenIddictApplicationManager>();
        var resourceManager = container.GetRequiredService<IamResourceManager>();
        const string resourceId = "test-resource";
        
        var resource = await applicationManager.FindByClientIdAsync(resourceId);
        if (resource is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                DisplayName = "Test Resource",
                ClientId = resourceId,
                RedirectUris = { new Uri("uri:signin"), new Uri("https://localhost:5001/signin-oidc") },
            }, stoppingToken);
        }
        
        var scopeManager = container.GetRequiredService<IOpenIddictScopeManager>();
        var fullaccessScope = await scopeManager.FindByNameAsync("fullaccess");
        if (fullaccessScope is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "fullaccess",
                Resources = { resourceId }
            }, stoppingToken);
        }
    }

    async Task CreateTestClientAsync(IServiceProvider container, CancellationToken stoppingToken)
    {
        var applicationManager = container.GetRequiredService<IOpenIddictApplicationManager>();

        const string clientId = "test";
        const string clientSecret = "secret";
        
        var client = await applicationManager.FindByClientIdAsync(clientId);
        if (client is not null)
            return;
        
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ApplicationType = OpenIddictConstants.ApplicationTypes.Web, // AspNet Core MVC
            ClientType = OpenIddictConstants.ClientTypes.Confidential, // Using backchannel auth
            DisplayName = "Test Client",
            ClientId = clientId,
            ClientSecret = clientSecret,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                
                OpenIddictConstants.Permissions.Prefixes.Scope + "fullaccess",
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Roles,

                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.ResponseTypes.Token,
                OpenIddictConstants.Permissions.ResponseTypes.CodeToken,
                OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken,
                OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken,
                OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken,
            },
            RedirectUris = { new Uri("uri:signin"), new Uri("https://localhost:5001/signin-oidc") },
        }, stoppingToken);

    }
}