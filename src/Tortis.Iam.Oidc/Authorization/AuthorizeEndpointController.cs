using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tortis.Iam.Oidc.Authorization;

// https://datatracker.ietf.org/doc/html/rfc6749#section-3.1

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)] // should be secured using the cookie created from the user logging in.
[Route(ENDPOINT)]
public class AuthorizeEndpointController : ControllerBase
{
    internal const string ENDPOINT = "oauth2/authorize";

    readonly ResponseTypeHandlerFactory _responseTypeHandlerFactory;

    public AuthorizeEndpointController(ResponseTypeHandlerFactory responseTypeHandlerFactory)
    { 
        _responseTypeHandlerFactory = responseTypeHandlerFactory;
    }

    [HttpGet]  public Task<IActionResult> Get([FromQuery] AuthorizeRequest authorizeRequest, CancellationToken cancellationToken) => Authorize(authorizeRequest, cancellationToken);
    [HttpPost] public Task<IActionResult> Post([FromForm] AuthorizeRequest authorizeRequest, CancellationToken cancellationToken) => Authorize(authorizeRequest, cancellationToken);

    async Task<IActionResult> Authorize(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(authorizeRequest.ResponseType))
            return Respond(new Uri(authorizeRequest.RedirectUri!), ErrorResponse.InvalidRequest("Missing required field response_type."));
        
        var handler = _responseTypeHandlerFactory.GetHandlerForResponseType(authorizeRequest.ResponseType, HttpContext.RequestServices);
        var response = await handler.HandleAsync(authorizeRequest, cancellationToken);
        return Respond(new Uri(authorizeRequest.RedirectUri!), response);
    }

    IActionResult Respond(Uri? redirectUri, object response)
    {
        if (redirectUri is null)
            return BadRequest(response);
        
        var redirectUriWithRepsonse = redirectUri.ToString();
        if (string.IsNullOrWhiteSpace(redirectUri.Query))
            redirectUriWithRepsonse += "?";
        else
            redirectUriWithRepsonse += "&";

        redirectUriWithRepsonse += GetQueryString(response);

        return Redirect(redirectUriWithRepsonse);
    }
    
    public string GetQueryString(object obj) {
        var properties = from p in obj.GetType().GetProperties()
            where p.GetValue(obj, null) != null
            select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

        return string.Join("&", properties.ToArray());
    }
}