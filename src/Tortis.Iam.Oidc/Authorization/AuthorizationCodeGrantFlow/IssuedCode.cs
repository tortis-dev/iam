namespace Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;

public class IssuedCode
{
    public string ClientId { get; set; }

    public string RedirectUri { get; set; }

    public string Code { get; set; }

    public DateTime ExpiresAt { get; set; }

    internal static IssuedCode GenerateFromRequest(AuthorizeRequest authorizeRequest)
    {
        return new IssuedCode
        {
            Code = Guid.NewGuid().ToString("N"),//TODO: Maybe something more "secure"
            ClientId = authorizeRequest.ClientId!,
            RedirectUri = authorizeRequest.RedirectUri!,
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromMinutes(10) //TODO: Make configurable
        };
    }
}