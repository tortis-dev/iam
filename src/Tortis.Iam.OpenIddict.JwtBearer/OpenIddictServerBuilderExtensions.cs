using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;
using OpenIddict.Server.Handlers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class OpenIddictServerBuilderExtensions
{
    /// <summary>
    /// Enables the JWT bearer authorization (Impersonation) flow.
    /// https://www.rfc-editor.org/rfc/rfc7523 Sections 2.1, 3, 3.1, 4 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static OpenIddictServerBuilder AllowJwtBearerAuthorizationFlow(this OpenIddictServerBuilder builder)
    {
        return builder.AllowJwtBearerAuthorizationFlow(_ => { });
    }

    /// <summary>
    /// Enables the JWT bearer authorization (Impersonation) flow.
    /// https://www.rfc-editor.org/rfc/rfc7523 Sections 2.1, 3, 3.1, 4 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static OpenIddictServerBuilder AllowJwtBearerAuthorizationFlow(this OpenIddictServerBuilder builder, Action<OpenIddictJwtBearerOptions> configure)
    {
        builder.AllowCustomFlow(JwtBearerGrantTypes.JwtBearer);
        builder.Services.Configure(configure);
        builder.AddEventHandler(ValidateJwtBearerGrant.Descriptor);
        builder.AddEventHandler(AttachJwtBearerPrincipal.Descriptor);
        return builder;
    }
}