// ReSharper disable InconsistentNaming
namespace Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;

class AuthorizationCodeResponse : IResponse
{
    public AuthorizationCodeResponse(string code, string? state = null)
    {
        this.code = code;
        this.state = state;
    }
    public string code { get; }
    public string? state { get; }
}