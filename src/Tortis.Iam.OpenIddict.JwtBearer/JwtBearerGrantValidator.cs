using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace OpenIddict.Server.Handlers;

public sealed class JwtBearerGrantValidator
{
    public async Task<JwtBearerValidationResult> ValidateAsync(
        string assertion,
        string clientId,
        OpenIddictJwtBearerOptions options,
        IOpenIddictJwtBearerIssuerProvider? issuerProvider = null)
    {
        if (string.IsNullOrEmpty(assertion))
        {
            return JwtBearerValidationResult.Failure(
                OpenIddictConstants.Errors.InvalidRequest,
                "The assertion parameter is required.");
        }

        var validationParameters = options.TokenValidationParameters.Clone();
        validationParameters.ClockSkew = options.ClockSkew;
        if (options.ValidateClientIdAsAudience)
            validationParameters.ValidAudience = clientId;

        if (issuerProvider is not null)
        {
            var issuers = await issuerProvider.GetTrustedIssuersAsync();
            validationParameters.ValidIssuers = (validationParameters.ValidIssuers ?? Enumerable.Empty<string>())
                .Union(issuers)
                .Distinct();
            
            if (validationParameters.ValidIssuers.Any())
            {
                validationParameters.ValidateIssuer = true;
            }
        }

        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(assertion, validationParameters, out var validatedToken);

            if (validatedToken is not System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwt)
            {
                return JwtBearerValidationResult.Failure(
                    OpenIddictConstants.Errors.InvalidGrant,
                    "The token is not a valid JWT.");
            }

            return JwtBearerValidationResult.Success(principal);
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            return JwtBearerValidationResult.Failure(
                OpenIddictConstants.Errors.InvalidGrant,
                "The JWT signing key could not be determined.");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return JwtBearerValidationResult.Failure(
                OpenIddictConstants.Errors.InvalidGrant,
                "The JWT signature is invalid.");
        }
        catch (SecurityTokenExpiredException)
        {
            return JwtBearerValidationResult.Failure(
                OpenIddictConstants.Errors.InvalidGrant,
                "The JWT has expired.");
        }
        catch (SecurityTokenNotYetValidException)
        {
            return JwtBearerValidationResult.Failure(
                OpenIddictConstants.Errors.InvalidGrant,
                "The JWT is not yet valid.");
        }
        catch (Exception)
        {
            return JwtBearerValidationResult.Failure(
                OpenIddictConstants.Errors.InvalidGrant,
                "The JWT validation failed.");
        }
    }
}