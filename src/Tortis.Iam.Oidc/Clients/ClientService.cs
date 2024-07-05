namespace Tortis.Iam.Oidc.Clients;

public class ClientService
{
    readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    public Task<Client?> FindByClientIdAsync(string clientId, CancellationToken cancellationToken)
    {
        return _repository.FindByClientIdAsync(clientId, cancellationToken);
    }
    
    public Task<Client> CreateAsync(Client client, CancellationToken cancellationToken)
    {
        foreach (var redirectUri in client.RedirectUris)
        {
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var validUri))
                throw new ArgumentException($"Redirect uri {redirectUri} is invalid. Value must be a well formed absolute uri.");
        }
        
        if (string.IsNullOrWhiteSpace(client.ClientId))
            client.ClientId = Guid.NewGuid().ToString();
        
        if (string.IsNullOrWhiteSpace(client.ClientSecret))
            client.ClientSecret = Guid.NewGuid().ToString();
        
        return _repository.CreateAsync(client, cancellationToken);
    }
}