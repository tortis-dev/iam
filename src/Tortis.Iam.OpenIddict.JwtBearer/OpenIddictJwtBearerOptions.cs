using Microsoft.IdentityModel.Tokens;

namespace OpenIddict.Server.Handlers;

public sealed class OpenIddictJwtBearerOptions
{
    /// <summary>
    /// Gets or sets the parameters used to validate the JWT assertion.
    /// </summary>
    public TokenValidationParameters TokenValidationParameters { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the client identifier should be validated as the audience.
    /// Default is true.
    /// </summary>
    public bool ValidateClientIdAsAudience { get; set; } = true;

    /// <summary>
    /// Gets or sets the clock skew applied during token validation.
    /// Default is 5 minutes.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
}
