using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Serilog;
using Xunit.Abstractions;

namespace OpenIdConnectTests;

public class ClientCredentialFlow: IClassFixture<ServerFixture>
{
    readonly ServerFixture _server;
    readonly ITestOutputHelper _output;

    public ClientCredentialFlow(ServerFixture server, ITestOutputHelper output)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(output).CreateLogger();
        _server = server;
        _output = output;
    }

    [Fact]
    public async Task ClientSecretBasic()
    {
        const string clientId = "test";
        const string clientSecret = "secret";
        
        var request = new HttpRequestMessage(HttpMethod.Post, "connect/token");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "scope", "fullaccess" }
        });
        var response = await _server.Client.SendAsync(request);
        _output.WriteLine(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task ClientSecretPost()
    {
        const string clientId = "test";
        const string clientSecret = "secret";
        
        var request = new HttpRequestMessage(HttpMethod.Post, "connect/token");

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "scope", "fullaccess" }
        });
        var response = await _server.Client.SendAsync(request);
        _output.WriteLine(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}