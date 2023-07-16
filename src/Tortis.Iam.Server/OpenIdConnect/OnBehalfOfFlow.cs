using System.IdentityModel.Tokens.Jwt;
using OpenIddict.Abstractions;

namespace Tortis.Iam.Server.OpenIdConnect;

public static class OnBehalfOfExtensions
{
    public const string GrantType = "urn:ietf:params:oauth:grant-type:token-exchange";
    public const string SubjectTokenType = "urn:ietf:params:oauth:token-type:access_token";
    
    public static class Permissions
    {
        public const string GrantType = $"gt:{OnBehalfOfExtensions.GrantType}";
    }

    public static string? GetSubjectToken(this OpenIddictRequest request)
    {
        return request.GetParameter("subject_token")?.Value as string;
    }
    
    public static JwtSecurityToken GetSubjectTokenAsJwtToken(this OpenIddictRequest request)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadJwtToken(request.GetSubjectToken());
    }
    
    public static string? GetSubjectTokenType(this OpenIddictRequest request)
    {
        return request.GetParameter("subject_token_type")?.Value as string;
    }
}