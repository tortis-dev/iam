using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authentication;

namespace Tortis.Iam.Oidc.Tests;

public static class Utilities
{
    public static Dictionary<string, string> QueryStringAsDictionary(this Uri uri)
    {
        return uri.Query.Split('&')
            .ToDictionary(
                kvp => kvp.Split('=')[0].TrimStart('?'),
                kvp =>HttpUtility.UrlDecode(kvp.Split('=')[1]));
    }

    public static void SetBasicAuthorizationHeader(this HttpRequestHeaders requestHeaders, string clientId, string clientSecret)
    {
        requestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", 
            Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));
    }

    public static (string, string) GetClientIdAndSecretFromAuthorizationHeader(this HttpRequestHeaders requestHeaders)
    {
        var authHeader = requestHeaders.Authorization?.ToString().Split(' ');

        if (authHeader is null || authHeader.Length <= 0 || authHeader[0] != "Basic")
            return (string.Empty, string.Empty);
        
        var decodedBasicAuthentication = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(authHeader[1])).Split(':');
        return (decodedBasicAuthentication[0], decodedBasicAuthentication[1]);
    }
}