namespace Tortis.Iam.Server.Components.OpenIdConnect;

/// <summary>
/// Represents a trusted external issuer (authority) for JWT bearer grants.
/// </summary>
public class TrustedIssuer
{
    public Guid Id { get; set; }

    /// <summary>
    /// The issuer string (e.g., https://accounts.google.com).
    /// </summary>
    public required string Issuer { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ModifiedOn { get; set; }
    public required string CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public string? ConcurrencyToken { get; set; }
}
