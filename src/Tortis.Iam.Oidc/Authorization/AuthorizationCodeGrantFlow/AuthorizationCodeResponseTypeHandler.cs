using Tortis.Iam.Oidc.Clients;

namespace Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;

/// <summary>
/// Invoked by calling the /authorize endpoint with response_type=code.
/// Supports the Authentication Code Grant Flow.
/// </summary>
public class AuthorizationCodeResponseTypeHandler : IResponseTypeHandler
{
    readonly ClientService _clientService;
    readonly IIssuedCodeRepository _issuedCodeRepository;

    public AuthorizationCodeResponseTypeHandler(ClientService clientService, IIssuedCodeRepository issuedCodeRepository)
    {
        _clientService = clientService;
        _issuedCodeRepository = issuedCodeRepository;
    }

    public string ResponseType => "code";

    public async Task<IResponse> HandleAsync(AuthorizeRequest authorizeRequest,
        CancellationToken cancellationToken)
    {
        //https://datatracker.ietf.org/doc/html/rfc6749#section-4.1
        /*   The authorization code MUST expire
             shortly after it is issued to mitigate the risk of leaks.  A
             maximum authorization code lifetime of 10 minutes is
             RECOMMENDED.  The client MUST NOT use the authorization code
             more than once.  If an authorization code is used more than
             once, the authorization server MUST deny the request and SHOULD
             revoke (when possible) all tokens previously issued based on
             that authorization code.  The authorization code is bound to
             the client identifier and redirection URI.
         */

        try
        {
            // response_type is required and must be set to "code"
            if (string.IsNullOrWhiteSpace(authorizeRequest.ResponseType))
                return ErrorResponse.InvalidRequest("Missing required field response_type.");

            // This should never happen, but will protect against consumers from doing bad things with this handler.
            if (authorizeRequest.ResponseType != ResponseType)
                throw new InvalidOperationException(
                    "Handler for the Authorization Code Grant called with different response type.");

            // client_id is required
            if (string.IsNullOrWhiteSpace(authorizeRequest.ClientId))
                return ErrorResponse.InvalidRequest("Missing required field client_id.");

            // TODO: redirect_uri is optional. May use redirect_uri registered for client. If a redirect_uri is provided,
            // the provided value MUST match one of the registered redirect_uris for the client.

            var client = await _clientService.FindByClientIdAsync(authorizeRequest.ClientId, cancellationToken);
            if (client is null)
                return ErrorResponse.UnauthorizedClient();

            if (!string.IsNullOrWhiteSpace(authorizeRequest.RedirectUri)
                && !client.RedirectUris.Contains(authorizeRequest.RedirectUri))
                return ErrorResponse.InvalidRequest("Invalid redirect_uri.");

            authorizeRequest.RedirectUri = client.RedirectUris.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authorizeRequest.RedirectUri))
                return ErrorResponse.InvalidRequest("Invalid redirect_uri.");

            var issuedCode = IssuedCode.GenerateFromRequest(authorizeRequest);
            await _issuedCodeRepository.SaveIssuedCodeAsync(issuedCode, cancellationToken);
            
            return new AuthorizationCodeResponse(issuedCode.Code, authorizeRequest.State);
        }
        catch (Exception)
        {
            return ErrorResponse.ServerError();
        }
    }
}