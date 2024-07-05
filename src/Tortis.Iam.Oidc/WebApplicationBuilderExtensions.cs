using Tortis.Iam.Oidc.Authorization;
using Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;
using Tortis.Iam.Oidc.Clients;

namespace Tortis.Iam.Oidc;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddTortisIamOidc(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<IStartupFilter, StartupFilter>();
        builder.Services.AddScoped<ClientService>();
        builder.Services.AddSingleton<ResponseTypeHandlerFactory>();
        builder.Services.AddSingleton<GrantTypeHandlerFactory>();
        builder.Services.AddTransient<AuthorizationCodeResponseTypeHandler>();
        builder.Services.AddTransient<AuthorizationCodeGrantTypeHandler>();

        builder.Services.AddSingleton<ISigningKeyAccessor, DevelopmentSigningKey>();
        return builder;
    }
}