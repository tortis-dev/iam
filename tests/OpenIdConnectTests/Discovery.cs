using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Serilog;
using Xunit.Abstractions;

namespace OpenIdConnectTests;

public class Discovery : IClassFixture<ServerFixture>
{
    private ServerFixture _server;

    public Discovery(ServerFixture server, ITestOutputHelper output)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(output).CreateLogger();
        _server = server;
    }

    [Fact]
    public async Task Endpoint()
    {
        var responseMessage = await _server.Client.GetAsync(".well-known/openid-configuration");
        Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
    }

    [Fact]
    public async Task SupportsClientCredentials()
    {
        var responseMessage = await _server.Client.GetAsync(".well-known/openid-configuration");

        var metadata = await responseMessage.Content.ReadFromJsonAsync<JsonObject>();
        var supportedGrantTypes = metadata?["grant_types_supported"]?.AsArray();
        var grant = supportedGrantTypes?.SingleOrDefault(n => n?.ToString().Equals("client_credentials") ?? false);
        Assert.NotNull(grant);
    }
    
    [Fact]
    public async Task SupportsAuthorizationCode()
    {
        var responseMessage = await _server.Client.GetAsync(".well-known/openid-configuration");

        var metadata = await responseMessage.Content.ReadFromJsonAsync<JsonObject>();
        var supportedGrantTypes = metadata?["grant_types_supported"]?.AsArray();
        var grant = supportedGrantTypes?.SingleOrDefault(n => n?.ToString().Equals("authorization_code") ?? false);
        Assert.NotNull(grant);
    }
    
    [Fact]
    public async Task SupportsRefreshToken()
    {
        var responseMessage = await _server.Client.GetAsync(".well-known/openid-configuration");

        var metadata = await responseMessage.Content.ReadFromJsonAsync<JsonObject>();
        var supportedGrantTypes = metadata?["grant_types_supported"]?.AsArray();
        var grant = supportedGrantTypes?.SingleOrDefault(n => n?.ToString().Equals("refresh_token") ?? false);
        Assert.NotNull(grant);
    }
    
    
    [Fact]
    public async Task SupportsOfflineAccess()
    {
        var responseMessage = await _server.Client.GetAsync(".well-known/openid-configuration");

        var metadata = await responseMessage.Content.ReadFromJsonAsync<JsonObject>();
        var supportedGrantTypes = metadata?["scopes_supported"]?.AsArray();
        var grant = supportedGrantTypes?.SingleOrDefault(n => n?.ToString().Equals("offline_access") ?? false);
        Assert.NotNull(grant);
    }
}