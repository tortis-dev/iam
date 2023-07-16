using System.IdentityModel.Tokens.Jwt;
using OpenIddict.Abstractions;

namespace Tortis.Iam.Server.OpenIdConnect;

public static class OnBehalfOfFlow
{
    public const string GrantType = "urn:ietf:params:oauth:grant-type:token-exchange";
    public const string SubjectTokenType = "urn:ietf:params:oauth:token-type:access_token";
    public const string Permission = $"gt:{OnBehalfOfFlow.GrantType}";

    public static bool IsTokenExchangeGrantType(this OpenIddictRequest request)
    {
        return request.GrantType == GrantType;
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