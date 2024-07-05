using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Tortis.Iam.Oidc.Clients;
using Tortis.Iam.Oidc.TokenExchange;

namespace Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;

public class AuthorizationCodeGrantTypeHandler : IGrantTypeHandler
{
    readonly IIssuedCodeRepository _issuedCodeRepository;
    readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationCodeGrantTypeHandler(IIssuedCodeRepository issuedCodeRepository, IHttpContextAccessor httpContextAccessor)
    {
        _issuedCodeRepository = issuedCodeRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public string GrantType => "authorization_code";
    
    public Task<ClaimsIdentity> HandleAsync(TokenRequest tokenRequest, CancellationToken cancellationToken)
    {
        // https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.3

        throw new NotImplementedException();
        // // grant_type is required and must be set to "code"
        // if (string.IsNullOrWhiteSpace(tokenRequest.grant_type))
        //     return ErrorResponse.InvalidRequest("Missing required field grant_type.");
        //
        // // This should never happen, but will protect against consumers from doing bad things with this handler.
        // if (tokenRequest.grant_type != GrantType)
        //     throw new InvalidOperationException(
        //         "Handler for the Authorization Code Grant called with different grant type.");
        //
        // // client_id is required
        // if (string.IsNullOrWhiteSpace(tokenRequest.client_id))
        //     return ErrorResponse.InvalidRequest("Missing required field client_id.");
        //
        // // code is required
        // if (string.IsNullOrWhiteSpace(tokenRequest.code))
        //     return ErrorResponse.InvalidRequest("Missing required field code.");
        //
        // if (string.IsNullOrWhiteSpace(tokenRequest.redirect_uri))
        //     return ErrorResponse.InvalidRequest("Missing required field redirect_uri.");
        //     
        // var issuedCode = await _issuedCodeRepository.FindIssuedCodeAsync(tokenRequest.code, cancellationToken);
        // if (issuedCode is null)
        //     return ErrorResponse.InvalidRequest();
        //
        // if (issuedCode.RedirectUri != tokenRequest.redirect_uri)
        //     return ErrorResponse.InvalidRequest("Invalid redirect_uri.");
        //     
        // if (issuedCode.ClientId != tokenRequest.client_id)
        //     return ErrorResponse.InvalidRequest("Invalid client.");
        //
        // if (issuedCode.ExpiresAt < DateTime.UtcNow)
        //     return ErrorResponse.InvalidRequest("Invalid code.");
        //
        // //TODO: Stronger key
        // var bytes = Guid.NewGuid().ToByteArray().Concat(Guid.NewGuid().ToByteArray()).ToArray();
        // var key = new SymmetricSecurityKey(bytes);
        // key.KeyId = "my-key";
        //
        // //var jsonWebKey = JsonWebKeyConverter.ConvertFromSymmetricSecurityKey(key);
        //
        // var user = _httpContextAccessor.HttpContext.User;
        //
        // var jwtTokenHandler = new JsonWebTokenHandler();
        // var accessTokenDescriptor = new SecurityTokenDescriptor
        // {
        //     Issuer = "http://sts.tortis.com",
        //     Subject = new ClaimsIdentity(new[] { new Claim("sub", user.Identity?.Name ?? tokenRequest.client_id ) }),
        //     Expires = DateTime.UtcNow + TimeSpan.FromHours(1),
        //     IssuedAt = DateTime.UtcNow,
        //     SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        // };
        // var accessToken = jwtTokenHandler.CreateToken(accessTokenDescriptor);
        //
        // var fakeUserId = Guid.NewGuid().ToString();
        // var subject = Base64UrlEncoder.Encode($"{fakeUserId}:{tokenRequest.client_id}");
        //
        // var idTokenDescriptor = new SecurityTokenDescriptor
        // {
        //     Audience = tokenRequest.client_id,
        //     Issuer = "http://sts.tortis.com",
        //     Subject = new ClaimsIdentity(new[]
        //     {
        //         new Claim("sub", subject ),
        //         new Claim("name", "Elmer Fudd" ),
        //         new Claim("email", "efudd@acme.com")
        //     }),
        //     Expires = DateTime.UtcNow + TimeSpan.FromHours(1),
        //     IssuedAt = DateTime.UtcNow,
        //     SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        // };
        //
        // var idToken = jwtTokenHandler.CreateToken(idTokenDescriptor);
        //
        // var refreshTokenDescriptor = new SecurityTokenDescriptor
        // {
        //     Issuer = "http://sts.tortis.com",
        //     Subject = new ClaimsIdentity(new[]
        //     {
        //         new Claim("sub", subject ),
        //         new Claim("name", "Elmer Fudd" ),
        //         new Claim("email", "efudd@acme.com")
        //     }),
        //     Expires = DateTime.UtcNow + TimeSpan.FromHours(1),
        //     IssuedAt = DateTime.UtcNow,
        //     SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        //     EncryptingCredentials = new EncryptingCredentials(key, SecurityAlgorithms.Aes256Encryption)
        // };
        //
        // var refreshToken = jwtTokenHandler.CreateToken(refreshTokenDescriptor);
        //
        // return new TokenResponse
        // {
        //     id_token = idToken,
        //     access_token = accessToken,
        //     refresh_token = refreshToken,
        //     expires_in = 3600,
        //     token_type = "Bearer"
        // };
    }
}