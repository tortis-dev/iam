using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;

using Tortis.Iam.Server.Components.Users;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

public class TokenEndpointController : ControllerBase
{
    private const string JwtBearerGrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
    /// <summary>
    /// To obtain an Access Token, an ID Token, and optionally a Refresh Token, the RP (Client) sends a Token Request to
    /// the Token Endpoint to obtain a Token Response, as described in Section 3.2 of OAuth 2.0 [RFC6749], when using
    /// the Authorization Code Flow.
    ///
    /// https://openid.net/specs/openid-connect-core-1_0.html#TokenEndpoint
    /// https://www.rfc-editor.org/rfc/rfc6749.html#section-3.2
    /// </summary>
    [HttpPost(TortisOpenIdConstants.TOKEN_ENDPOINT)]
    [AllowAnonymous]
    public async Task<IActionResult> Token()
    {
        // Note:
        // The client credentials are automatically validated by OpenIddict. If client_id or client_secret are invalid,
        // this action won't be invoked.
        
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        
        // TODO: Validate the resource is valid if the request contains a resource parameter.

        ClaimsPrincipal? claimsPrincipal;

        if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Subject (sub) is a required field, we use the client id as the subject identifier here.
            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

            // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
            identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

            claimsPrincipal = new ClaimsPrincipal(identity);
            
            claimsPrincipal.SetScopes(request.GetScopes());
            
            // TODO: Set resources
        }
        else if (request.IsAuthorizationCodeGrantType())
        {
            // Retrieve the claims principal stored in the authorization code
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            // var user = await _iamUserManager.FindByIdAsync(claimsPrincipal.GetClaim(ClaimTypes.NameIdentifier));
            // var roles = await _iamUserManager.GetRolesAsync(user);
            // foreach (var role in roles)
            //     claimsPrincipal.AddClaim(OpenIddictConstants.Claims.Role, role);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the refresh token.
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            // var user = await _iamUserManager.GetUserAsync(claimsPrincipal);
            // var roles = await _iamUserManager.GetRolesAsync(user);
            // foreach (var role in roles)
            //     claimsPrincipal.AddClaim(OpenIddictConstants.Claims.Role, role);
        }
        else if (string.Equals(request.GrantType, JwtBearerGrantType, StringComparison.Ordinal))
        {
            // JWT Bearer grant validation and principal attachment is handled by 
            // ValidateJwtBearerGrant and AttachJwtBearerPrincipal handlers.
            claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            if (claimsPrincipal is null)
            {
                throw new InvalidOperationException("JWT Bearer grant validation did not produce a principal.");
            }
        }
        else
        {
            throw new InvalidOperationException("The specified grant type is not supported.");
        }
        
        if (claimsPrincipal is null)
            throw new InvalidOperationException("Unable to create claims principal.");
        
        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}