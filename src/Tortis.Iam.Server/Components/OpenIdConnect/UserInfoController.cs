// "Licensed under GPL-3."

using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using Tortis.Iam.Server.Components.Users;
using Tortis.Iam.Server.Data;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
public class UserInfoController : ControllerBase
{
    UserManager<IamUser> _userManager;

    public UserInfoController(UserManager<IamUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("connect/userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        var user = await _userManager.FindByNameAsync(User.Identity!.Name!);
        return Ok(new
        {
            iss = User.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Issuer)?.Value,
            aud = User.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Audience)?.Value ?? User.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.ClientId)?.Value,
            sub = User.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Subject)?.Value,
            name = User.Identity.Name,
            preferred_username = user?.UserName,
            email = user?.Email,
            email_verified = user?.EmailConfirmed,
            phone_number = user?.PhoneNumber,
            phone_number_verified = user?.PhoneNumberConfirmed
        });
    }
}