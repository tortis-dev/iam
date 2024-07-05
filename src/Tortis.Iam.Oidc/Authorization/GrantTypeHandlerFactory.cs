using System.Collections.Concurrent;
using System.Security.Claims;
using Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;
using Tortis.Iam.Oidc.TokenExchange;

namespace Tortis.Iam.Oidc.Authorization;

public interface IGrantTypeHandler
{
    string GrantType { get; }
    Task<ClaimsIdentity> HandleAsync(TokenRequest tokenRequest, CancellationToken cancellationToken);
}

public class GrantTypeHandlerFactory
{
    readonly ConcurrentDictionary<string, Type> _handlers;
    
    public GrantTypeHandlerFactory()
    {
        _handlers = new ConcurrentDictionary<string, Type>();
        _handlers.TryAdd(GrantTypes.AuthorizationCode, typeof(AuthorizationCodeGrantTypeHandler));
    }

    public IGrantTypeHandler GetHandlerForGrantType(string grantType, IServiceProvider containerScope)
    {
        if (!_handlers.TryGetValue(grantType, out var handlerType))
            throw new ApplicationException("Grant type not registered.");
        
        return (IGrantTypeHandler)containerScope.GetRequiredService(handlerType);
    }
}