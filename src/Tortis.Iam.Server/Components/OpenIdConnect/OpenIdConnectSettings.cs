namespace Tortis.Iam.Server.Components.OpenIdConnect;

public class OpenIdConnectSettings
{
    public bool EnablePasswordGrant { get; set; }
    public bool EnableHybridFlow { get; set; }
    public bool EnableImplicitFlow { get; set; }
    public bool EnableClientCredentialsFlow { get; set; } = true;
    public bool EnableAuthorizationCodeFlow { get; set; } = true;
    public bool RequirePkceGlobally { get; set; } = true;
    public bool EnableRefreshTokenFlow { get; set; } = true;
    public bool EnableDeviceFlow { get; set; }
    public bool EnableUserinfoEndpoint { get; set; }
    public bool EnableEndSessionEndpoint { get; set; }
    public bool EnableRevocationEndpoint { get; set; }
    public bool EnableDiscoveryEndpoint { get; set; } = true;
    public bool EnableTokenEndpoint { get; set; } = true;
    public bool EnableAccessTokenEncryption { get; set; } = true;
}