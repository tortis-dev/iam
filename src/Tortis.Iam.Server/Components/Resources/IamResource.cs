// Licensed under GPL-3 (https://www.gnu.org/licenses/gpl-3.0.en.html)

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Tortis.Iam.Server.Components.Resources;

/// <summary>
/// Represents a protected resource. In OAuth2.0, a protected resource is an API.
/// https://datatracker.ietf.org/doc/html/rfc6749#section-1.1
/// This allows for the use of the resource parameter in the authorization request:
/// https://datatracker.ietf.org/doc/html/rfc8707
/// </summary>
public class IamResource
{
    public IamResource(string urn)
    {
        Urn = urn;
    }
    
    public Guid Id { get; set; }
    [Url]
    public string Urn { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ModifiedOn { get; set; }
    public required string CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public string? ConcurrencyToken { get; set; }
}
