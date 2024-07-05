using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tortis.Iam.Oidc.Authorization;

namespace Tortis.Iam.Oidc.Configuration;

[AllowAnonymous]
[Route(".well-known")]
public class WellKnownConfigurationController : ControllerBase
{
    [HttpGet("openid-configuration")]
    public IActionResult OpenIdConfiguration(CancellationToken cancellationToken)
    {
        var issuer = new Uri($"{Request.Scheme}://{Request.Host}");
        var config = new WellKnownConfiguration
        {
            Issuer = issuer,
            AuthorizationEndpoint = new Uri(issuer, AuthorizeEndpointController.ENDPOINT),
            TokenEndpoint = new Uri(issuer, "/token"),
            UserinfoEndpoint = new Uri(issuer, "/userinfo")
        };
        return Ok(config);
    }
}