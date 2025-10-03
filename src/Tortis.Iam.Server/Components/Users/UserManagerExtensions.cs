using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Tortis.Iam.Server.Data;

namespace Tortis.Iam.Server.Components.Users;

static class UserManagerExtensions
{
    public static async Task<string> GenerateEncodedPasswordResetTokenAsync(this UserManager<IamUser> userManager, IamUser user)
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
    }

    public static string GeneratePasswordResetLink(this NavigationManager navigationManager, string resetToken)
    {   
        return navigationManager.GetUriWithQueryParameters(
            navigationManager.ToAbsoluteUri("account/resetpassword").AbsoluteUri,
            new Dictionary<string, object?> { ["code"] = resetToken });
    }
}