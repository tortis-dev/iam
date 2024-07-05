using System.Security.Claims;
using Tortis.Iam.Oidc.TokenExchange;

namespace Tortis.Iam.Oidc.Authorization.ClientCredentialsGrantFlow;

class ClientCredentialsGrantTypeHandler : IGrantTypeHandler
{
    ISigningKeyAccessor _signingKeyAccessor;
    IEncryptionKeyAccessor _encryptionKeyAccessor;

    public ClientCredentialsGrantTypeHandler(ISigningKeyAccessor signingKeyAccessor, IEncryptionKeyAccessor encryptionKeyAccessor)
    {
        _signingKeyAccessor = signingKeyAccessor;
        _encryptionKeyAccessor = encryptionKeyAccessor;
    }

    public string GrantType => "client_credentials";
    public async Task<ClaimsIdentity> HandleAsync(TokenRequest tokenRequest, CancellationToken cancellationToken)
    {
        // grant_type is required and must be set to "client_credentials"
        if (string.IsNullOrWhiteSpace(tokenRequest.grant_type))
            throw new OidcException(ErrorResponse.InvalidRequest("Missing required field grant_type."));

        // This should never happen, but will protect against consumers from doing bad things with this handler.
        if (tokenRequest.grant_type != GrantType)
            throw new InvalidOperationException(
                "Handler for the Authorization Code Grant called with different grant type.");
        
        // client_id is required
        if (string.IsNullOrWhiteSpace(tokenRequest.client_id))
            throw new OidcException( ErrorResponse.InvalidRequest("Missing required field client_id."));
        
        // client_secret is required
        if (string.IsNullOrWhiteSpace(tokenRequest.client_secret))
            throw new OidcException( ErrorResponse.InvalidRequest("Missing required field client_secret."));
        
        // response_type is required
        if (string.IsNullOrWhiteSpace(tokenRequest.response_type))
            throw new OidcException( ErrorResponse.InvalidRequest("Missing required field response_type."));
        
        // response_type should be token
        if (!string.Equals(tokenRequest.response_type, "access_token"))
            throw new OidcException( ErrorResponse.InvalidRequest("Invalid response_type."));

        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaim(new Claim("sub", tokenRequest.client_id));

        return claimsIdentity;
        //return new TokenGenerator(_signingKeyAccessor, _encryptionKeyAccessor).GenerateTokens(claimsIdentity, true);
    }
}