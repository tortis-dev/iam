using System.Net.Http.Json;
using System.Text.Json.Nodes;
using FluentAssertions;

namespace Tortis.Iam.Oidc.Tests;

// https://openid.net/wordpress-content/uploads/2018/06/OpenID-Connect-Conformance-Profiles.pdf
public class ConfigurationTests : IClassFixture<HostFixure>
{
    readonly HostFixure _hostFixture;

    public ConfigurationTests(HostFixure hostFixture)
    {
        _hostFixture = hostFixture;
    }

    [Fact(DisplayName = "Publishes openid-configuration discovery information")]
    public async Task PublishesOpenIdConfiguration()
    {
        var host = _hostFixture.Host;
        var client = host.CreateClient();
        var response = await client.GetAsync("/.well-known/openid-configuration");
        response.IsSuccessStatusCode.Should().BeTrue();
    }
    
    [Fact(DisplayName = "Config has issuer")]
    public async Task ConfigHasIssuer()
    {
        var host = _hostFixture.Host;
        var client = host.CreateClient();
        var response = await client.GetAsync("/.well-known/openid-configuration");
        var config = await response.Content.ReadFromJsonAsync<JsonObject>();
        config.Should().ContainKey("issuer");
    }
    
    [Fact(DisplayName = "Discovered issuer matches openid-configuration path prefix")]
    public async Task IssueMatchesConfigPrefix()
    {
        var host = _hostFixture.Host;
        var client = host.CreateClient();
        var response = await client.GetAsync("/.well-known/openid-configuration");
        var config = await response.Content.ReadFromJsonAsync<JsonObject>();
        client.BaseAddress?.ToString().Should().StartWith(config?["issuer"]?.GetValue<string>());
    }
    
    [Fact(DisplayName = "Config has authorization_endpoint")]
    public async Task HasAuthorizationEndpoint()
    {
        var host = _hostFixture.Host;
        var client = host.CreateClient();
        var response = await client.GetAsync("/.well-known/openid-configuration");
        var config = await response.Content.ReadFromJsonAsync<JsonObject>();
        config.Should().ContainKey("authorization_endpoint");
    }
    
    [Fact(DisplayName = "Config has token_endpoint")]
    public async Task HasTokenEndpoint()
    {
        var host = _hostFixture.Host;
        var client = host.CreateClient();
        var response = await client.GetAsync("/.well-known/openid-configuration");
        var config = await response.Content.ReadFromJsonAsync<JsonObject>();
        config.Should().ContainKey("token_endpoint");
    }
    
    [Fact(DisplayName = "Config has userinfo_endpoint")]
    public async Task HasUserinfoEndpoint()
    {
        var host = _hostFixture.Host;
        var client = host.CreateClient();
        var response = await client.GetAsync("/.well-known/openid-configuration");
        var config = await response.Content.ReadFromJsonAsync<JsonObject>();
        config.Should().ContainKey("userinfo_endpoint");
    }
}