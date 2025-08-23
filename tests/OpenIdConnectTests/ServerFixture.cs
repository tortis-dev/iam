using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tortis.Iam.Server.Data;

namespace OpenIdConnectTests;

public class ServerFixture : IDisposable
{
    public HttpClient Client { get; }
    internal WebApplicationFactory<Program>  Factory { get; }
    
    readonly string _databaseFile;
    public ServerFixture()
    {
        _databaseFile = $"{Guid.NewGuid()}.db";
        // var container = new ServiceCollection()
        //     .AddDbContext<IamDbContext>(options => options.UseSqlite($"Filename={_databaseFile}"))
        //     .BuildServiceProvider();
        //
        // using var scope = container.CreateScope();
        // var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
        // db.Database.EnsureCreated();
        
        Factory = new WebApplicationFactory<Program>();
        Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<DbContextOptionsBuilder<IamDbContext>>(_ =>
                {
                    var options = new DbContextOptionsBuilder<IamDbContext>();
                    options.UseSqlite($"Filename={_databaseFile}");
                    return options;
                });
            });
            builder.ConfigureLogging(logging =>
            {
                logging.AddSerilog(Log.Logger);
            });
        });
       
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