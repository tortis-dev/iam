using OpenIddict.Server.Handlers;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

public sealed class DatabaseTrustedAuthorityProvider : ITrustedAuthorityProvider
{
    public Task<IEnumerable<TrustedAuthority>> GetTrustedIssuersAsync()
    {
        return Task.FromResult<IEnumerable<TrustedAuthority>>([new TrustedAuthority("https://accounts.example.com")]);
    }
}
