using OpenIddict.Server.Handlers;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

public sealed class DatabaseOpenIddictJwtBearerIssuerProvider : IOpenIddictJwtBearerIssuerProvider
{
    public Task<IEnumerable<TrustedAuthority>> GetTrustedIssuersAsync()
    {
        return Task.FromResult<IEnumerable<TrustedAuthority>>([new TrustedAuthority("https://accounts.example.com")]);
    }
}
