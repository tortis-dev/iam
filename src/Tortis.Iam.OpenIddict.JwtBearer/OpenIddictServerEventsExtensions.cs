using System.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace OpenIddict.Server.Handlers;

public static class OpenIddictServerEventsExtensions
{
    public static ClaimsPrincipal? GetJwtBearerPrincipal(this OpenIddictServerEvents.ProcessSignInContext context)
    {
        if (context.Transaction.Properties.TryGetValue("jwt_bearer_principal", out var value) && value is ClaimsPrincipal principal)
        {
            return principal;
        }
        return null;
    }
}