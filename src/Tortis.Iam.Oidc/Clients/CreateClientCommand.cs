using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tortis.Iam.Oidc.Clients;

public static class CreateClientCommandEndpointExtension
{
    public static void MapCreateClientCommand(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/clients",
            [Authorize] 
            [ProducesResponseType(typeof(Client), 200)]
            ([FromServices] IClientRepository repository, [FromBody] Client client, CancellationToken cancellationToken) =>
                CreateClientCommand.ExecuteAsync(repository, client, cancellationToken));
    }
}

static class CreateClientCommand
{
    public static async Task<IResult> ExecuteAsync(IClientRepository repository, Client client, CancellationToken cancellationToken)
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
        
        return  Results.Ok(await repository.CreateAsync(client, cancellationToken));
    }
}