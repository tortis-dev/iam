using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace Tortis.Iam.Server.Data;

sealed class SetupDefaultAdmin : BackgroundService
{
    IServiceProvider _container;
    ILogger<SetupDefaultAdmin> _logger;

    public SetupDefaultAdmin(IServiceProvider container, ILogger<SetupDefaultAdmin> logger)
    {
        _container = container;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await using var scope = _container.CreateAsyncScope();

        await CreateAdministratorRoleAsync(scope.ServiceProvider);
        await CreateAdminUserAsync(scope.ServiceProvider, stoppingToken);
        await CreateTestResourceAsync(scope.ServiceProvider, stoppingToken);
        await CreateTestClientAsync(scope.ServiceProvider, stoppingToken);
    }

    async Task CreateAdministratorRoleAsync(IServiceProvider container)
    {
        var roleManager = container.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await roleManager.CreateAsync(new IdentityRole<Guid>("Administrator"));
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
                UserName = username,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(admin, defaultPassword);

            if (!result.Succeeded)
            {
                _logger.LogCritical(result.ToString());
                Environment.Exit(1);
            }

            await userManager.AddToRoleAsync(admin, "Administrator");
        }
    }

    async Task CreateTestResourceAsync(IServiceProvider container, CancellationToken stoppingToken)
    {
        var applicationManager = container.GetRequiredService<IOpenIddictApplicationManager>();

        const string resourceId = "test-resource";
        
        var resource = await applicationManager.FindByClientIdAsync(resourceId);
        if (resource is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                //ClientType = OpenIddictConstants.ClientTypes.Confidential,
                DisplayName = "Test Resource",
                ClientId = resourceId,
                RedirectUris = { new Uri("uri:signin") },
                //ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
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
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
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
                
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.ResponseTypes.Token,
                OpenIddictConstants.Permissions.ResponseTypes.CodeToken,
                OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken,
                OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken,
                OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken,
            },
            RedirectUris = { new Uri("uri:signin") },
            ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
        }, stoppingToken);

    }
}