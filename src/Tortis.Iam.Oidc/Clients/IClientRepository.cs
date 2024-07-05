namespace Tortis.Iam.Oidc.Clients;

public interface IClientRepository
{
    Task<Client?> FindByClientIdAsync(string clientId, CancellationToken cancellationToken);
    Task<Client> CreateAsync(Client client, CancellationToken cancellationToken);
}