using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Tortis.Iam.Oidc.Authorization;

namespace Tortis.Iam.Oidc.TokenExchange;

//Should accept Basic for server-server, and cookie (the logged in user) for code exchange.
[Authorize(AuthenticationSchemes = $"Basic,{CookieAuthenticationDefaults.AuthenticationScheme}")]
[Route(ENDPOINT)]
public class TokenController : ControllerBase
{
    public const string ENDPOINT = "oauth2/token";

    static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    readonly GrantTypeHandlerFactory _handlerFactory;

    public TokenController(GrantTypeHandlerFactory handlerFactory)
    {
        _handlerFactory = handlerFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Token([FromForm] TokenRequest tokenRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tokenRequest.grant_type))
            return Respond(tokenRequest, ErrorResponse.InvalidRequest("Missing required field grant_type."), HttpStatusCode.BadRequest);
        
        var handler = _handlerFactory.GetHandlerForGrantType(tokenRequest.grant_type, HttpContext.RequestServices);
        var token = await handler.HandleAsync(tokenRequest, cancellationToken);
        return Respond(tokenRequest, token);
    }
    
    IActionResult Respond(TokenRequest request, object response, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        // grant_type=authorization_code, response_type=token ==> Redirect
        // grant_type=client_credentials, response_type=token ==> OK
        // grant_type=password, response_type=token ==> OK
        // grant_type=refresh_token, response_type=token ==> OK
        
        Response.Headers.CacheControl = CacheControlHeaderValue.NoStoreString;
        
        if (request.redirect_uri is null || request.grant_type != GrantTypes.AuthorizationCode)
        {
            Response.StatusCode = response is ErrorResponse ? 400 : (int)statusCode;
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            return Content(json, "application/json", Encoding.UTF8);
        }

        var uri = new Uri(request.redirect_uri);
        var redirectUriWithRepsonse = request.redirect_uri;
        if (string.IsNullOrWhiteSpace(uri.Query))
            redirectUriWithRepsonse += "?";
        else
            redirectUriWithRepsonse += "&";

        redirectUriWithRepsonse += CreateQueryString(response);

        return Redirect(redirectUriWithRepsonse);
    }
    
    public string CreateQueryString(object obj) {
        var properties = from p in obj.GetType().GetProperties()
            where p.GetValue(obj, null) != null
            select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

        return string.Join("&", properties.ToArray());
    }
}