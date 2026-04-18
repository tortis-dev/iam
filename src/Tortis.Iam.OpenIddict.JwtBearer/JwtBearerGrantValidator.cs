using System.Collections.Concurrent;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace OpenIddict.Server.Handlers;

public sealed class JwtBearerGrantValidator
{
    private static readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _configManagers = new();

    private static readonly ConcurrentDictionary<string, string> _issuerAuthorities = new();
    
    public async Task<JwtBearerValidationResult> ValidateAsync(
        string assertion,
        string clientId,
        OpenIddictJwtBearerOptions options,
        ITrustedAuthorityProvider issuerProvider)
    {
        if (string.IsNullOrEmpty(assertion))
        {
            return JwtBearerValidationResult.Failure(
                OpenIddictConstants.Errors.InvalidRequest,
                "The assertion parameter is required.");
        }

        var validationParameters = options.TokenValidationParameters.Clone();

        var subjectToken = new JsonWebToken(assertion);
        
        if (!_configManagers.TryGetValue(subjectToken.Issuer, out var configManager))
        {
            // First, we the latest set of trusted authorities.
            IEnumerable<TrustedAuthority> trustedAuthorities = await issuerProvider.GetTrustedIssuersAsync();
            
            foreach (var authority in trustedAuthorities)
            {
                // If we've already configured a manager for this authority, skip it.'
                if (_issuerAuthorities.ContainsKey(authority.Authority))
                    continue;

                var metadataAddress = authority.GetMetadataAddress();
                var cm = new ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataAddress,
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());

                var config = await cm.GetBaseConfigurationAsync(CancellationToken.None);
                _configManagers.TryAdd(config.Issuer, cm);
                _issuerAuthorities.TryAdd(authority.Authority, config.Issuer);
            }
            
            _configManagers.TryGetValue(subjectToken.Issuer, out configManager);
        }
        
        if (configManager is null)
        {
            return JwtBearerValidationResult.Failure(
                OpenIddictConstants.Errors.InvalidToken,
                $"Issuer '{subjectToken.Issuer}' is not trusted.");
        }
        
        validationParameters.ConfigurationManager = configManager;
        
        validationParameters.ClockSkew = options.ClockSkew;
        
        if (options.ValidateClientIdAsAudience)
            validationParameters.ValidAudience = clientId;

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