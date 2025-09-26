// Licensed under GPL-3 (https://www.gnu.org/licenses/gpl-3.0.en.html)

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Tortis.Iam.Server.Components.Users;

public sealed class IamUserManager : UserManager<IamUser>
{
    public IamUserManager(IUserStore<IamUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<IamUser> passwordHasher, IEnumerable<IUserValidator<IamUser>> userValidators, IEnumerable<IPasswordValidator<IamUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<IamUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public override Task<IdentityResult> DeleteAsync(IamUser user)
    {
        if (string.Equals(user.UserName, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(IdentityResult.Failed([new IdentityError()
            {
                Code = "SystemUserCannotBeDeleted",
                Description = "The Admin user cannot be deleted. It is a system user. If you want to disable the account for security reasons, you can lock the account instead."
            }]));
        }

        return base.DeleteAsync(user);
    }
}