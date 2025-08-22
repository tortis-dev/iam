using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Serilog;
using Xunit.Abstractions;

namespace OpenIdConnectTests;

public class ClientCredentialFlow: IClassFixture<ServerFixture>, IAsyncLifetime
{
    readonly ServerFixture _server;
    readonly ITestOutputHelper _output;

    public ClientCredentialFlow(ServerFixture server, ITestOutputHelper output)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(output).CreateLogger();
        _server = server;
        _output = output;
    }
    
    const string CLIENT_ID = "test_client_credentials";
    const string CLIENT_SECRET = "secret";
    const string SCOPE = "fullaccess";
    
    [Fact]
    public async Task ClientSecretBasic()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "connect/token");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{CLIENT_ID}:{CLIENT_SECRET}")));

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "scope", SCOPE }
        });
        var response = await _server.Client.SendAsync(request);
        _output.WriteLine(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task ClientSecretPost()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "connect/token");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", CLIENT_ID },
            { "client_secret", CLIENT_SECRET },
            { "scope", SCOPE }
        });
        var response = await _server.Client.SendAsync(request);
        _output.WriteLine(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Runs once per Fact
    public async Task InitializeAsync()
    {
        await using var asyncServiceScope = _server.Factory.Services.CreateAsyncScope();
        var applicationManager = asyncServiceScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
       
        var client = await applicationManager.FindByClientIdAsync(CLIENT_ID);
        if (client is not null)
            return;
        
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ApplicationType = OpenIddictConstants.ApplicationTypes.Web, // AspNet Core MVC
            ClientType = OpenIddictConstants.ClientTypes.Confidential, // Using backchannel auth
            DisplayName = "Test Client for Client Credentials Flow",
            ClientId = CLIENT_ID,
            ClientSecret = CLIENT_SECRET,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.Prefixes.Scope + SCOPE,
                OpenIddictConstants.Permissions.ResponseTypes.Token,
            }
        }, CancellationToken.None);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}