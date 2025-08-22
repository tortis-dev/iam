using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

public class AuthorizeEndpointController : ControllerBase
{
    readonly IOpenIddictScopeManager _scopeManager;
    
    public AuthorizeEndpointController(IOpenIddictScopeManager scopeManager)
    {
        _scopeManager = scopeManager;
    }

    /// <summary>
    /// The Authorization Endpoint performs Authentication of the End-User. This is done by sending the User Agent to
    /// the Authorization Server's Authorization Endpoint for Authentication and Authorization, using request parameters
    /// defined by OAuth 2.0 and additional parameters and parameter values defined by OpenID Connect.
    ///
    /// https://openid.net/specs/openid-connect-core-1_0.html#AuthorizationEndpoint
    /// https://www.rfc-editor.org/rfc/rfc6749.html#section-3.1
    /// </summary>
    [HttpGet("connect/authorize")]
    [HttpPost("connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        
        // Retrieve the user principal stored in the authentication cookie.
        var authenticationResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // If the user principal can't be extracted, redirect the user to the login page.
        if (!authenticationResult.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // HACK: This should never be null. Do we want to handle it better?
        var nameClaimValue = authenticationResult.Principal!.Identity!.Name ?? Guid.NewGuid().ToString();
        var identifier = authenticationResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? nameClaimValue;
        // Create a new claims principal
        var claims = new List<Claim>
        {                                                                               
            new(OpenIddictConstants.Claims.Subject, identifier),
            new(OpenIddictConstants.Claims.Name, nameClaimValue)
        };

        var roles = authenticationResult.Principal.Claims.Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);
        
        foreach (var role in roles)
        {
            claims.Add(new Claim(OpenIddictConstants.Claims.Role, role));
        }
        
        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var resources = _scopeManager.ListResourcesAsync(request.GetScopes()).ToBlockingEnumerable();
        claimsIdentity.SetScopes(request.GetScopes());
        claimsIdentity.SetResources(resources);
        claimsIdentity.SetDestinations(_ => [OpenIddictConstants.Destinations.AccessToken]);

        return SignIn(new ClaimsPrincipal(claimsIdentity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

}