using OpenIddict.Abstractions;

namespace OpenIddict.Server.Handlers;

public static class OpenIddictRequestExtensions
{
    public static bool IsJwtBearerGrantType(this OpenIddictRequest request)
    {
        return string.Equals(
            request.GrantType,
            JwtBearerGrantTypes.JwtBearer,
            StringComparison.Ordinal);
    }
}