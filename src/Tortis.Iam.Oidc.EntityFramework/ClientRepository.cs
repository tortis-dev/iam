using Microsoft.EntityFrameworkCore;
using Tortis.Iam.Oidc.Clients;

namespace Tortis.Iam.Oidc.EntityFramework;

sealed class ClientRepository<TDbContext> : RepositoryBase<TDbContext>, IClientRepository
    where TDbContext : DbContext
{

    public ClientRepository(TDbContext context) : base(context)
    { }
    
    public async Task<Client?> FindByClientIdAsync(string clientId, CancellationToken cancellationToken)
    {
        return await Context.FindAsync<Client>(clientId);
    }

    public async Task<Client> CreateAsync(Client client, CancellationToken cancellationToken)
    {
        await Context.AddAsync(client, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return client;
    }


}