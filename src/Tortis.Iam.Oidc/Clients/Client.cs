namespace Tortis.Iam.Oidc.Clients;

public class Client
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    
    public string? DisplayName { get; set; }

    public HashSet<string> RedirectUris { get; init; } = new();
}
