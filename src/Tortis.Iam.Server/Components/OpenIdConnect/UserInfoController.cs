// "Licensed under GPL-3."

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using Tortis.Iam.Server.Components.Users;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
public class UserInfoController : ControllerBase
{
    UserManager<IamUser> _userManager;
    ILogger<UserInfoController> _logger;

    public UserInfoController(UserManager<IamUser> userManager)
    {
        _userManager = userManager;
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
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            
            // This _should_ never happen. The edge case would be the user was deleted while the token was still valid, 
            // or the token was tampered with and somehow passed validation.
            // TODO: A SIEM event should be raised here.
            if (user is null)
                return BadRequest(new { error = "invalid_token" });
            
            return Ok(new
            {
                iss = User.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Issuer)?.Value,
                aud =
                    User.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Audience)?.Value ??
                    User.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.ClientId)?.Value,
                sub = User.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Subject)?.Value,
                name = User.Identity.Name,
                preferred_username = user?.UserName,
                email = user?.Email,
                email_verified = user?.EmailConfirmed,
                phone_number = user?.PhoneNumber,
                phone_number_verified = user?.PhoneNumberConfirmed,
                updated_at = user?.ModifiedOn?.ToUnixTimeSeconds() ?? user?.CreatedOn.ToUnixTimeSeconds()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user info.");
            return  Problem("Error retrieving user info.", title: "Error retrieving user info.", statusCode: 500);
        }
    }
}