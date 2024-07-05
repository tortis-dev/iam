using System.Text.Json.Serialization;
using System.Web;

namespace Tortis.Iam.Oidc;

/// <summary>
/// OAuth2.0 Error Response
/// https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.2.1
/// </summary>
sealed class ErrorResponse : IResponse
{
    public static ErrorResponse InvalidRequest(string? description = null) => 
        new ErrorResponse("invalid_request")
        {
            error_description = description
        };
    
    public static ErrorResponse UnsupportedResponseType(string? description = null) => 
        new ErrorResponse("unsupported_response_type")
        {
            error_description = description
        };

    public static ErrorResponse UnauthorizedClient(string? description = null) =>
        new ErrorResponse("unauthorized_client")
        {
            error_description = description
        };
    
    public static ErrorResponse ServerError(string? description = null) =>
        new ErrorResponse("server_error")
        {
            error_description = description
        };
    
    ErrorResponse(string error)
    {
        this.error = error;
    }
    
    public string error { get; set; }
    public string? error_description { get; set; }
    public string? error_uri { get; set; }
}