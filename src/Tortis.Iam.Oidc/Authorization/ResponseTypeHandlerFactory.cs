using System.Collections.Concurrent;
using Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;

namespace Tortis.Iam.Oidc.Authorization;

public interface IResponseTypeHandler
{
    string ResponseType { get; }
    Task<IResponse> HandleAsync(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken);
}

public class ResponseTypeHandlerFactory
{
    readonly ConcurrentDictionary<string, Type> _handlers;
    
    public ResponseTypeHandlerFactory()
    {
        _handlers = new ConcurrentDictionary<string, Type>();
        _handlers.TryAdd("code", typeof(AuthorizationCodeResponseTypeHandler));
    }

    public IResponseTypeHandler GetHandlerForResponseType(string responseType, IServiceProvider containerScope)
    {
        if (!_handlers.TryGetValue(responseType, out var handlerType))
            throw new ApplicationException("Response type not registered.");
        
        return (IResponseTypeHandler)containerScope.GetRequiredService(handlerType);
    }
}