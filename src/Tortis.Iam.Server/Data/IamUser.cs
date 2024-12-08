using Microsoft.AspNetCore.Identity;

namespace Tortis.Iam.Server.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class IamUser : IdentityUser<Guid>
{
    public bool AccountLocked => LockoutEnabled && LockoutEnd is not null;
}