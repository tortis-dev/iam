// Licensed under GPL-3 (https://www.gnu.org/licenses/gpl-3.0.en.html)

using Microsoft.AspNetCore.Identity;

namespace Tortis.Iam.Server.Components.Roles;

public class IamRole : IdentityRole<Guid>
{
    public IamRole(string name) : base(name)
    {
    }

    public string? Description { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ModifiedOn { get; set; }
    public required string CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}