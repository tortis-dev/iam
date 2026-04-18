using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Server;

namespace OpenIddict.Server.Handlers;

public static class OpenIddictServerBuilderExtensions
{
    public static OpenIddictServerBuilder AddJwtBearerGrant(this OpenIddictServerBuilder builder)
    {
        return builder.AddJwtBearerGrant(_ => { });
    }

    public static OpenIddictServerBuilder AddJwtBearerGrant(this OpenIddictServerBuilder builder, Action<OpenIddictJwtBearerOptions> configure)
    {
        builder.AllowCustomFlow("urn:ietf:params:oauth:grant-type:jwt-bearer");
        builder.Services.Configure(configure);
        builder.AddEventHandler(ValidateJwtBearerGrant.Descriptor);
        builder.AddEventHandler(AttachJwtBearerPrincipal.Descriptor);
        return builder;
    }
}