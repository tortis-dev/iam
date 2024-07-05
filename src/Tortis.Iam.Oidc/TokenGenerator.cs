using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Tortis.Iam.Oidc.TokenExchange;

namespace Tortis.Iam.Oidc;

class TokenGenerator
{
    ISigningKeyAccessor _signingKeyAccessor;
    IEncryptionKeyAccessor _encryptionKeyAccessor;

    public TokenGenerator(ISigningKeyAccessor signingKeyAccessor, IEncryptionKeyAccessor encryptionKeyAccessor)
    {
        _signingKeyAccessor = signingKeyAccessor;
        _encryptionKeyAccessor = encryptionKeyAccessor;
    }

    public TokenResponse GenerateTokens(ClaimsIdentity claimsIdentity, bool includeRefreshToken)
    {
        var issuedAt = DateTime.UtcNow;

        var accessTokenTtl = 3600; //TODO: Config
        
        var jwtTokenHandler = new JsonWebTokenHandler();
        var accessTokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = "http://sts.tortis.com",
            Subject = claimsIdentity,
            Expires = issuedAt + TimeSpan.FromSeconds(accessTokenTtl), 
            IssuedAt = issuedAt,
            SigningCredentials = new SigningCredentials(_signingKeyAccessor.GetKey(), SecurityAlgorithms.HmacSha256)
        };
        var accessToken = jwtTokenHandler.CreateToken(accessTokenDescriptor);

        SecurityTokenDescriptor? refreshTokenDescriptor = null;
        if (includeRefreshToken)
            refreshTokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "http://sts.tortis.com",
                Subject = claimsIdentity,
                Expires = issuedAt + TimeSpan.FromDays(14), //TODO: Config
                IssuedAt = issuedAt,
                SigningCredentials = new SigningCredentials(_signingKeyAccessor.GetKey(), SecurityAlgorithms.HmacSha256),
                EncryptingCredentials = new EncryptingCredentials(_encryptionKeyAccessor.GetKey(), JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512)
            };

        var refreshToken = jwtTokenHandler.CreateToken(refreshTokenDescriptor);
        
        return new TokenResponse
        {
            access_token = accessToken,
            refresh_token = refreshToken,
            expires_in = accessTokenTtl,
            token_type = "Bearer"
        };
    }
}