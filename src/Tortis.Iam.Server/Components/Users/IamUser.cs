using Microsoft.AspNetCore.Identity;

namespace Tortis.Iam.Server.Components.Users;

// Add profile data for application users by adding properties to the ApplicationUser class
public class IamUser : IdentityUser<Guid>
{
    public bool AccountLocked => LockoutEnabled && LockoutEnd is not null;
    public string Name => $"{GivenName} {FamilyName}".Trim();

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
}