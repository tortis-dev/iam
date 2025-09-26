// "Licensed under GPL-3."

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using Tortis.Iam.Server.Components.Users;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

/// <summary>
/// The UserInfo Endpoint is an OAuth 2.0 Protected Resource that returns Claims about the authenticated End-User. 
/// https://openid.net/specs/openid-connect-core-1_0.html#UserInfo
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
public class UserInfoController : ControllerBase
{
    readonly UserManager<IamUser> _userManager;
    readonly ILogger<UserInfoController> _logger;

    public UserInfoController(UserManager<IamUser> userManager, ILogger<UserInfoController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet(TortisOpenIdConstants.USERINFO_ENDPOINT)]
    [HttpPost(TortisOpenIdConstants.USERINFO_ENDPOINT)]
    public async Task<IActionResult> UserInfo()
    {
        // When the access token is for machine-to-machine communication, i.e. the client_credentials grant, there is no
        // user associated with the access token, so the userinfo endpoint cannot be invoked.
        // TODO: Maybe a SIEM event should be raised here?
        if (User.Identity?.Name is null)
            return BadRequest(new
            {
                error = "invalid_token",
                error_description = "The access token is not a valid user access token."
            });

        try
        {
            var userId = User.GetClaim(OpenIddictConstants.Claims.Subject);
            if (userId is null)
                return BadRequest(new { error = "invalid_token" });
            
            var user = await _userManager.FindByIdAsync(userId);
            
            // This _should_ never happen. The edge case would be the user was deleted while the token was still valid, 
            // or the token was tampered with and somehow passed validation.
            // TODO: A SIEM event should be raised here.
            if (user is null)
                return BadRequest(new { error = "invalid_token" });
            
            return Ok(new
            {
                iss = User.GetClaim(OpenIddictConstants.Claims.Issuer),
                aud =
                    User.GetClaim(OpenIddictConstants.Claims.Audience) ??
                    User.GetClaim(OpenIddictConstants.Claims.ClientId) ??
                    User.GetClaim(OpenIddictConstants.Claims.Issuer),
                sub = User.GetClaim(OpenIddictConstants.Claims.Subject),
                name = user.ToString(),
                preferred_username = user.UserName,
                email = user.Email,
                email_verified = user.EmailConfirmed,
                phone_number = user.PhoneNumber,
                phone_number_verified = user.PhoneNumberConfirmed,
                updated_at = user.ModifiedOn?.ToUnixTimeSeconds() ?? user.CreatedOn.ToUnixTimeSeconds(),
                given_name = user.GivenName,
                family_name = user.FamilyName,
                
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user info.");
            return  Problem("Error retrieving user info.", title: "Error retrieving user info.", statusCode: 500);
        }
    }
}