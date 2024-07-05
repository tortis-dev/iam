using System.Text.Json.Serialization;

namespace Tortis.Iam.Oidc.Configuration;

# nullable disable
public class WellKnownConfiguration
{
    [JsonPropertyName("issuer")]
    public Uri Issuer { get; set; }
    [JsonPropertyName("authorization_endpoint")]
    public Uri AuthorizationEndpoint { get; set; }
    [JsonPropertyName("token_endpoint")]
    public Uri TokenEndpoint { get; set; }
    [JsonPropertyName("userinfo_endpoint")]
    public Uri UserinfoEndpoint { get; set; }
}