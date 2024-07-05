using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace Tortis.Iam.Oidc.Tests;

public class TokenGenerationTests
{
    ITestOutputHelper _output;

    public TokenGenerationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestEncryption()
    {
        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaim(new Claim("sub", "test"));
        var issuedAt = DateTime.UtcNow;

        var bytes = Guid.NewGuid().ToByteArray().Concat(Guid.NewGuid().ToByteArray()).ToArray();
        
        // Signing key is 256 bits
        var signingKey = new SymmetricSecurityKey(bytes);
        signingKey.KeyId = "test-key";

        // Encryption key must be 512 bits
        var encryptionKey = new SymmetricSecurityKey(bytes.Concat(bytes).ToArray());
        encryptionKey.KeyId = "test-enc";

        var encryptingCredentials = 
            new EncryptingCredentials(encryptionKey, 
                JwtConstants.DirectKeyUseAlg, 
                SecurityAlgorithms.Aes256CbcHmacSha512);
       
        var jwtTokenHandler = new JsonWebTokenHandler();
        var refreshTokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = "http://sts.tortis.com",
            Subject = claimsIdentity,
            Expires = issuedAt + TimeSpan.FromDays(14),
            IssuedAt = issuedAt,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            EncryptingCredentials = encryptingCredentials
        };
        
        var refreshToken = jwtTokenHandler.CreateToken(refreshTokenDescriptor);
        //var encryptedToken = jwtTokenHandler.EncryptToken(refreshToken, encryptingCredentials);
    }
}
