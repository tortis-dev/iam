using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Tortis.Iam.Oidc.Authorization;

class AuthorizeRequestBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var authorizationRequest = new AuthorizeRequest();
        
        IEnumerable<KeyValuePair<string, StringValues>> parameters;
        if (bindingContext.BindingSource?.Id == "Query")
            parameters = bindingContext.HttpContext.Request.Query;
        else if (bindingContext.HttpContext.Request.HasFormContentType)
            parameters = bindingContext.HttpContext.Request.Form;
        else
            return Task.CompletedTask;

        foreach (var parameter in parameters)
        {
            authorizationRequest.Extensions.Add(parameter.Key, parameter.Value);
            
            switch (parameter.Key)
            {
                case "client_id":
                    authorizationRequest.ClientId = parameter.Value;
                    break;
                case "client_secret":
                    authorizationRequest.ClientSecret = parameter.Value;
                    break;
                case "response_type":
                    authorizationRequest.ResponseType = parameter.Value;
                    break;
                case "redirect_uri":
                    authorizationRequest.RedirectUri = parameter.Value;
                    break;
                case "state":
                    authorizationRequest.State = parameter.Value;
                    break;
            }
        }

        //TODO: I don't think this goes here. The clientId is required in request regardless of the authentication method.
        
        //https://datatracker.ietf.org/doc/html/rfc6749#section-2.3.1
        // The authorization server MUST support the HTTP Basic authentication scheme for authenticating
        // clients that were issued a client password.
        var authHeader = bindingContext.HttpContext.Request.Headers.Authorization;
        if (authHeader.Count > 0 && authHeader[0] == "Basic")
        {
            if (!string.IsNullOrWhiteSpace(authorizationRequest.ClientId))
            {
                //TODO: The client MUST NOT use more than one authentication method in each request.
            }
            
            var decodedBasicAuthentication = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(authHeader[1])).Split(':');
            authorizationRequest.ClientId = decodedBasicAuthentication[0];
            authorizationRequest.ClientSecret = decodedBasicAuthentication[1];
        }

        bindingContext.Result = ModelBindingResult.Success(authorizationRequest);
        return Task.CompletedTask;
    }
}