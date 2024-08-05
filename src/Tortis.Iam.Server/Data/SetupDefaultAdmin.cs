using Microsoft.AspNetCore.Identity;

namespace Tortis.Iam.Server.Data;

sealed class SetupDefaultAdmin : BackgroundService
{
    private IServiceProvider _container;
    private ILogger<SetupDefaultAdmin> _logger;
    public SetupDefaultAdmin(IServiceProvider container, ILogger<SetupDefaultAdmin> logger)
    {
        _container = container;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string username = "Admin";
        const string defaultPassword = "Admin1234!";
        await using var scope = _container.CreateAsyncScope();
        
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var store = scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
        var normalizedUsername = userManager.NormalizeName(username);
        var admin = await store.FindByNameAsync(normalizedUsername, stoppingToken);
        
        if (admin is not null)
            return;

        admin = new ApplicationUser();
        await store.SetUserNameAsync(admin, username, stoppingToken);

        admin.EmailConfirmed = true;
        var result = await userManager.CreateAsync(admin, defaultPassword);

        if (!result.Succeeded)
        {
            _logger.LogCritical(result.ToString());
            Environment.Exit(1);
        }
    }
}