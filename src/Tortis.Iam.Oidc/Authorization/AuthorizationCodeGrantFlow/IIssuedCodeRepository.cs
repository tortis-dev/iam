namespace Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;

public interface IIssuedCodeRepository
{
    Task<IssuedCode?> FindIssuedCodeAsync(string code, CancellationToken cancellationToken);

    Task SaveIssuedCodeAsync(IssuedCode issuedCode, CancellationToken cancellationToken);
}