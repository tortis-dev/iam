using System.Security.Claims;

namespace OpenIddict.Server.Handlers;

public sealed class JwtBearerValidationResult
{
    public bool IsRejected { get; private init; }
    public string? Error { get; private init; }
    public string? ErrorDescription { get; private init; }
    public ClaimsPrincipal? Principal { get; private init; }

    public static JwtBearerValidationResult Success(ClaimsPrincipal principal) => new()
    {
        Principal = principal
    };

    public static JwtBearerValidationResult Failure(string error, string? description = null) => new()
    {
        IsRejected = true,
        Error = error,
        ErrorDescription = description
    };
}