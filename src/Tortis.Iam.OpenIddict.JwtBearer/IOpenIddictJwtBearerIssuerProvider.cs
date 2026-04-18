namespace OpenIddict.Server.Handlers;

/// <summary>
/// Provides a way to dynamically fetch trusted issuers for JWT bearer validation.
/// </summary>
public interface IOpenIddictJwtBearerIssuerProvider
{
    /// <summary>
    /// Gets the list of trusted issuers.
    /// </summary>
    /// <returns>A list of trusted issuer strings.</returns>
    Task<IEnumerable<string>> GetTrustedIssuersAsync();
}
