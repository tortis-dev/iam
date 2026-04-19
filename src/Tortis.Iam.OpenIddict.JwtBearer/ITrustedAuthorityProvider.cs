namespace OpenIddict.Abstractions;

/// <summary>
/// Provides a way to dynamically fetch trusted authorities for JWT bearer validation.
/// </summary>
public interface ITrustedAuthorityProvider
{
    /// <summary>
    /// Gets the list of trusted issuers.
    /// </summary>
    /// <returns>A list of trusted issuer strings.</returns>
    Task<IEnumerable<TrustedAuthority>> GetTrustedIssuersAsync();
}

public record TrustedAuthority(string Authority, string? MetadataAddress = null)
{
    public string GetMetadataAddress()
    {
        if (string.IsNullOrWhiteSpace(MetadataAddress))
            return $"{Authority}/.well-known/openid-configuration";
        
        return MetadataAddress;
    }
}
