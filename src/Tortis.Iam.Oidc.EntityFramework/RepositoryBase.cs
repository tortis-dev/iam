using Microsoft.EntityFrameworkCore;

namespace Tortis.Iam.Oidc.EntityFramework;

abstract class RepositoryBase<TDbContext> where TDbContext : DbContext
{
    protected RepositoryBase(TDbContext context)
    {
        Context = context;
    }
    
    protected TDbContext Context { get; }
}