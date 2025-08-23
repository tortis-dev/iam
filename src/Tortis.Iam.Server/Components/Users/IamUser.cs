using Microsoft.AspNetCore.Identity;

namespace Tortis.Iam.Server.Components.Users;

// Add profile data for application users by adding properties to the ApplicationUser class
public sealed class IamUser : IdentityUser<Guid>
{
    public bool AccountLocked => LockoutEnabled && LockoutEnd is not null;
    public string Name => $"{GivenName} {FamilyName}".Trim();

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ModifiedOn { get; set; }
    public required string CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
}