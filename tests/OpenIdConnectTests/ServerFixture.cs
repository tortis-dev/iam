using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Tortis.Iam.Server.Data;

namespace OpenIdConnectTests;

public class ServerFixture : IDisposable
{
    public HttpClient Client { get; }
    internal AppFactory  Factory { get; }
    
    readonly string _databaseFile;
    
    public ServerFixture()
    {
        _databaseFile = Guid.NewGuid().ToString();

        Factory = new AppFactory();

        Factory.ConfigureTestConfiguration(cfg => cfg.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", $"Filename={_databaseFile}.db" }
        }));
        
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost/")
        });
    }

    public void Dispose()
    {
        foreach (var dbFile in Directory.GetFiles($".", $"{_databaseFile}*"))
            File.Delete(dbFile);
    }
}

/// <summary>
/// https://github.com/dotnet/aspnetcore/issues/37680#issuecomment-1331559463
/// </summary>
class AppFactory : WebApplicationFactory<Program>
{
    private Action<IConfigurationBuilder>? _action;

    /// <summary>
    /// Configuration overrides for integration tests.
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureTestConfiguration(Action<IConfigurationBuilder> configure)
    {
        _action += configure;
    }

    protected override IWebHostBuilder? CreateWebHostBuilder()
    {
        if (_action is { } a)
        {
            // Set this so that the async context flows
            TestConfiguration.Create(a);
        }

        return base.CreateWebHostBuilder();
    }
}