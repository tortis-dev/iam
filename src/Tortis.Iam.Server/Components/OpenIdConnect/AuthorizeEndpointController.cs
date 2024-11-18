using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Tortis.Iam.Server.Data;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

public class AuthorizeEndpointController : ControllerBase
{
    readonly IOpenIddictScopeManager _scopeManager;


    public AuthorizeEndpointController(IOpenIddictScopeManager scopeManager)
    {
        _scopeManager = scopeManager;
    }

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

        
        // Create a new claims principal
        var claims = new List<Claim>
        {
            new(OpenIddictConstants.Claims.Subject, authenticationResult.Principal.Identity.Name),
            new(OpenIddictConstants.Claims.Name, authenticationResult.Principal.Identity.Name)
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