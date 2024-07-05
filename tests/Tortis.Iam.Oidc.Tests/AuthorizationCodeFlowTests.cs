using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Tortis.Iam.Oidc.Clients;
using Xunit.Abstractions;

namespace Tortis.Iam.Oidc.Tests;

public class AuthorizationCodeFlowTests : IClassFixture<HostFixure>
{
    readonly HostFixure _hostFixture;
    readonly ITestOutputHelper _output;

    public AuthorizationCodeFlowTests(HostFixure hostFixture, ITestOutputHelper output)
    {
        _hostFixture = hostFixture;
        _output = output;
    }

    async Task CreateClient(string clientId, string clientSecret, string redirectUri)
    {
        var httpClient = _hostFixture.Host.CreateClient(new WebApplicationFactoryClientOptions());
        
        var testClient = new Client
            { ClientId = $"{clientId}", ClientSecret = $"{clientSecret}", RedirectUris = { $"{redirectUri}" } };
        var createClientResponse = await httpClient.PostAsJsonAsync("/api/clients", testClient);
        _output.WriteLine($"Created test client with status code: {createClientResponse.StatusCode}");
        createClientResponse.StatusCode.Should().Be(HttpStatusCode.OK);    
    }
    
    [Fact(DisplayName = "Request with response_type=code")]
    public async Task GetAuthorizationCode()
    {
        const string redirectUri = "http://op-response-code-authorize-only.com";
        const string clientSecret = "foobar";
        string clientId = Guid.NewGuid().ToString();

        await CreateClient(clientId, clientSecret, redirectUri);
        
        var httpClient = _hostFixture.Host.CreateClient(new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = false
        });
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));
        
        var response = await httpClient.GetAsync($"/oauth2/authorize?client_id={clientId}&client_secret={clientSecret}&response_type=code&redirect_uri={redirectUri}");

        var authResponseQueryStringParameters = response.Headers.Location?.QueryStringAsDictionary();

        if (response.StatusCode != HttpStatusCode.Redirect)
            _output.WriteLine(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().StartWith(redirectUri);
        authResponseQueryStringParameters.Should().ContainKey("code");
        authResponseQueryStringParameters?["code"].Should().NotBeNullOrWhiteSpace();
        _output.WriteLine($"Code received: {authResponseQueryStringParameters["code"]}");
    }
    
    [Fact]
    public async Task ExchangeCodeForAccessToken()
    {
        const string redirectUri = "http://exchange-authcode-for-token.com";
        const string clientSecret = "foobar";
        var clientId = Guid.NewGuid().ToString();
        
        await CreateClient(clientId, clientSecret, redirectUri);
        
        var httpClient = _hostFixture.Host.CreateClient(new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = false
        });
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

        var authCodeResponse = await httpClient.GetAsync($"/oauth2/authorize?client_id={clientId}&client_secret={clientSecret}&response_type=code&redirect_uri={redirectUri}");
        if (!authCodeResponse.IsSuccessStatusCode)
            _output.WriteLine(await authCodeResponse.Content.ReadAsStringAsync());
        
        authCodeResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        
        var authResponse = authCodeResponse.Headers.Location?.QueryStringAsDictionary();

        var code = authResponse?["code"]!;
        
        var tokenResponse = await httpClient.PostAsync("/oauth2/token", 
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"client_id", clientId},
                {"response_type", "token"},
                {"code", code},
                {"redirect_uri", redirectUri}
            }));

        if (tokenResponse.StatusCode != HttpStatusCode.Redirect)
            _output.WriteLine(await tokenResponse.Content.ReadAsStringAsync());

        var query = tokenResponse.Headers.Location.QueryStringAsDictionary();
        foreach (var kvp in query)
            _output.WriteLine("{0}={1}", kvp.Key, kvp.Value);
        
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        query.Should().ContainKey("access_token");
        query.Should().ContainKey("token_type");
        query.Should().ContainKey("expires_in");

        //TODO: Validate the access_token
        var jwtTokenHander = new JsonWebTokenHandler();
        var jwtToken = jwtTokenHander.ReadJsonWebToken(query["access_token"]);
        
    }
}