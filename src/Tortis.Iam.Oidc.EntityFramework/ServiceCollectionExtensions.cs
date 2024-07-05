using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;
using Tortis.Iam.Oidc.Clients;

namespace Tortis.Iam.Oidc.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTortisIamEntityFramework<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IClientRepository, ClientRepository<TDbContext>>();
        services.AddScoped<IIssuedCodeRepository, IssuedCodeRepository<TDbContext>>();
        return services;
    }
}