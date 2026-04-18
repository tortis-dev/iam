using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace OpenIddict.Server.Handlers;

internal sealed class ValidateJwtBearerGrant : IOpenIddictServerHandler<OpenIddictServerEvents.ProcessAuthenticationContext>
{
    private readonly IOptionsMonitor<OpenIddictJwtBearerOptions> _options;
    private readonly IOpenIddictJwtBearerIssuerProvider? _issuerProvider;

    public ValidateJwtBearerGrant(
        IOptionsMonitor<OpenIddictJwtBearerOptions> options,
        IOpenIddictJwtBearerIssuerProvider? issuerProvider = null)
    {
        _options = options;
        _issuerProvider = issuerProvider;
    }

    public static OpenIddictServerHandlerDescriptor Descriptor { get; } =
        OpenIddictServerHandlerDescriptor.CreateBuilder<OpenIddictServerEvents.ProcessAuthenticationContext>()
            .AddFilter<OpenIddictServerHandlerFilters.RequireGrantTypePermissionsEnabled>()
            .AddFilter<OpenIddictServerHandlerFilters.RequireClientIdParameter>()
            .UseSingletonHandler<ValidateJwtBearerGrant>()
            .SetOrder(10000)
            .SetType(OpenIddictServerHandlerType.Custom)
            .Build();

    public async ValueTask HandleAsync(OpenIddictServerEvents.ProcessAuthenticationContext context)
    {
        if (!context.Request.IsJwtBearerGrantType())
        {
            return;
        }

        if (string.IsNullOrEmpty(context.Request.Assertion))
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidRequest,
                description: "The assertion parameter is required.");
            return;
        }

        var validator = new JwtBearerGrantValidator();
        var result = await validator.ValidateAsync(
            context.Request.Assertion,
            context.ClientId ?? string.Empty,
            _options.CurrentValue,
            _issuerProvider);

        if (result.IsRejected)
        {
            context.Reject(error: result.Error, description: result.ErrorDescription);
            return;
        }

        context.Transaction.Properties["jwt_bearer_principal"] = result.Principal;
    }
}