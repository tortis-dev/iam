using OpenIddict.Abstractions;

namespace OpenIddict.Server;

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