using Microsoft.EntityFrameworkCore;
using Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;

namespace Tortis.Iam.Oidc.EntityFramework;

sealed class IssuedCodeRepository<TDbContext> : RepositoryBase<TDbContext>, IIssuedCodeRepository
    where TDbContext : DbContext
{
    public IssuedCodeRepository(TDbContext context) : base(context)
    { }
    
    public async Task<IssuedCode?> FindIssuedCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await Context.FindAsync<IssuedCode>(code);
    }

    public async Task SaveIssuedCodeAsync(IssuedCode issuedCode, CancellationToken cancellationToken)
    {
        await Context.AddAsync(issuedCode, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }
}