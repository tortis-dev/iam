using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tortis.Iam.Oidc.Tests;

public class ConfigurationTests
{
    [Fact(DisplayName = "Publishes openid-configuration discovery information")]
    public async Task DiscoveryConfig()
    {
        var host = new WebApplicationFactory<Program>();
        var client = host.CreateClient();
        var response = await client.GetAsync("/.well-known/openid-configuration");
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}