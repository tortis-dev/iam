using Microsoft.AspNetCore.Identity;

using Tortis.Iam.Server.Components.Users;
using Tortis.Iam.Server.Data;

namespace Tortis.Iam.Server.Components.Account;

sealed class IdentityUserAccessor(
    UserManager<IamUser> userManager,
    IdentityRedirectManager redirectManager)
{
    public async Task<IamUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser",
                $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}