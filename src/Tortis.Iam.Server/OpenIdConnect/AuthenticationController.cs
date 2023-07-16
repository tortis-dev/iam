using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Tortis.Iam.Server.OpenIdConnect;

public class AuthenticationController : ControllerBase
{
    public const string TOKEN_ENDPOINT = "oauth2/token";
    public const string AUTHORIZE_ENDPOINT = "oauth2/authorize";

    readonly TokenService _tokenService;

    public AuthenticationController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    [HttpPost(TOKEN_ENDPOINT)]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var claimsPrincipal = await _tokenService.GetTokenAsync(request);
        
        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(claimsPrincipal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet(AUTHORIZE_ENDPOINT)]
    [HttpPost(AUTHORIZE_ENDPOINT)]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // If the user principal can't be extracted, redirect the user to the login page.
        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // Create a new claims principal
        var claims = new List<Claim>
        {
            // 'subject' claim which is required
            new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Identity.Name),
            new Claim("some claim", "some value").SetDestinations(OpenIddictConstants.Destinations.AccessToken)
        };

        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Set requested scopes (this is not done automatically)
        claimsPrincipal.SetScopes(request.GetScopes());

        // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}