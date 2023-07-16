using System.Security.Claims;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Abstractions;

namespace Tortis.Iam.Server.OpenIdConnect;

public class TokenService
{
    public Task<ClaimsPrincipal> GetTokenAsync(OpenIddictRequest request)
    {
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(request.GetScopes());

        if (request.IsTokenExchangeGrantType())
        {
            ExchangeTokenFlow(request, identity);
        }
        else if (request.IsClientCredentialsGrantType())
        {
            ClientCredentialFlow(request, identity);
        }
        else
        {
            throw new OpenIddictExceptions.ProtocolException("The specified grant type is not supported.");
        }
        
        //TODO: Implement ability to add custom claims.
        
        return Task.FromResult(claimsPrincipal);
    }

    void ClientCredentialFlow(OpenIddictRequest request, ClaimsIdentity identity)
    {
        identity.AddClaim(
            OpenIddictConstants.Claims.Subject, 
            request.ClientId ?? throw new OpenIddictExceptions.ProtocolException("Missing client_id."));
    }
    
    void ExchangeTokenFlow(OpenIddictRequest request, ClaimsIdentity identity)
    {
        // See: https://datatracker.ietf.org/doc/html/rfc8693
        
        var subjectTokenType = request.GetSubjectTokenType();
        if (!OnBehalfOfFlow.SubjectTokenType.Equals(subjectTokenType,
                StringComparison.InvariantCultureIgnoreCase))
            throw new InvalidOperationException(
                $"Subject token type must be {OnBehalfOfFlow.SubjectTokenType}");
        
        var subjectToken = request.GetSubjectTokenAsJwtToken();
        identity.AddClaim(OpenIddictConstants.Claims.Subject, subjectToken.Subject);
    }
}