using Microsoft.EntityFrameworkCore;
using OpenIddict.Server.Handlers;
using Tortis.Iam.Server.Data;

namespace Tortis.Iam.Server.Components.OpenIdConnect;

public sealed class DatabaseOpenIddictJwtBearerIssuerProvider : IOpenIddictJwtBearerIssuerProvider
{
    public Task<IEnumerable<string>> GetTrustedIssuersAsync()
    {
        return Task.FromResult<IEnumerable<string>>(["https://accounts.example.com"]);
    }
}
