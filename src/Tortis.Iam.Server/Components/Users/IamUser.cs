using Microsoft.AspNetCore.Identity;

namespace Tortis.Iam.Server.Components.Users;

// Add profile data for application users by adding properties to the ApplicationUser class
public sealed class IamUser : IdentityUser<Guid>
{
    public bool AccountLocked => LockoutEnabled && LockoutEnd > DateTimeOffset.UtcNow;
    
    /// <summary>
    /// End-User's full name in displayable form
    /// </summary>
    public string Name => $"{GivenName} {FamilyName}".Trim();

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ModifiedOn { get; set; }
    public required string CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    
    /// <summary>
    /// Given name(s) or first name(s) of the End-User. Note that in some cultures, people can have multiple given names;
    /// all can be present, with the names being separated by space characters. 
    /// </summary>
    public string? GivenName { get; set; }
    
    /// <summary>
    /// Surname(s) or last name(s) of the End-User. Note that in some cultures, people can have multiple family names or
    /// no family name; all can be present, with the names being separated by space characters. 
    /// </summary>
    public string? FamilyName { get; set; }

    public void LockUser() => LockoutEnd = DateTimeOffset.MaxValue;
    public void UnlockUser() => LockoutEnd = DateTimeOffset.MinValue;

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Name) ? UserName! : Name;
    }
}