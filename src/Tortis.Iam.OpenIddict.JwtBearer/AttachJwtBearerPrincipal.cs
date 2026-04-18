using System.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace OpenIddict.Server.Handlers;

internal sealed class AttachJwtBearerPrincipal : IOpenIddictServerHandler<OpenIddictServerEvents.ProcessSignInContext>
{
    public static OpenIddictServerHandlerDescriptor Descriptor { get; } =
        OpenIddictServerHandlerDescriptor.CreateBuilder<OpenIddictServerEvents.ProcessSignInContext>()
            .AddFilter<OpenIddictServerHandlerFilters.RequireGrantTypePermissionsEnabled>()
            .AddFilter<OpenIddictServerHandlerFilters.RequireClientIdParameter>()
            .UseSingletonHandler<AttachJwtBearerPrincipal>()
            .SetOrder(10500)
            .SetType(OpenIddictServerHandlerType.Custom)
            .Build();

    public ValueTask HandleAsync(OpenIddictServerEvents.ProcessSignInContext context)
    {
        if (!context.Request.IsJwtBearerGrantType())
        {
            return ValueTask.CompletedTask;
        }

        if (context.Principal is not null)
        {
            return ValueTask.CompletedTask;
        }

        if (context.Transaction.Properties.TryGetValue("jwt_bearer_principal", out var value) && value is ClaimsPrincipal principal)
        {
            context.Principal = principal;
        }

        return ValueTask.CompletedTask;
    }
}
