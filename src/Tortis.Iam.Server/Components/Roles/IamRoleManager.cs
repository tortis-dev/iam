// Licensed under GPL-3 (https://www.gnu.org/licenses/gpl-3.0.en.html)

using Microsoft.AspNetCore.Identity;

namespace Tortis.Iam.Server.Components.Roles;

sealed class IamRoleManager : RoleManager<IamRole>
{
    public IamRoleManager(IRoleStore<IamRole> store, IEnumerable<IRoleValidator<IamRole>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<IamRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }

    public override Task<IdentityResult> DeleteAsync(IamRole role)
    {
        if (string.Equals(role.Name, Authorization.ADMINISTRATORS, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(IdentityResult.Failed([new IdentityError()
            {
                Code = "SystemRoleCannotBeDeleted",
                Description = "The administrators role cannot be deleted. This is a system role."
            }]));
        }
        
        return base.DeleteAsync(role);
    }
}