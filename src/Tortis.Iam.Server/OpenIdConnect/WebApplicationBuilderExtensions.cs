using OpenIddict.Abstractions;

namespace Tortis.Iam.Server.OpenIdConnect;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddOpenIdConnect(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<TokenService>();
        
        // OpenIdDict for handling OIDC flows
        builder.Services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<BoundedContext>())
            .AddServer(options =>
            {
                options.AllowClientCredentialsFlow();
                options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
                options.AllowRefreshTokenFlow();
                options.AllowCustomFlow(OnBehalfOfFlow.GrantType);

                options.SetAuthorizationEndpointUris(AuthenticationController.AUTHORIZE_ENDPOINT);
                options.SetTokenEndpointUris(AuthenticationController.TOKEN_ENDPOINT);
                options.SetUserinfoEndpointUris("/userinfo");
                
                options.UseAspNetCore()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough();


                options.DisableAccessTokenEncryption();
                
                if (builder.Environment.IsDevelopment())
                {
                    options.AddDevelopmentEncryptionCertificate();
                    options.AddDevelopmentSigningCertificate();
                }
            });
        return builder;
    }
}