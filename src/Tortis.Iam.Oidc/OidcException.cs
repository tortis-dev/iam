namespace Tortis.Iam.Oidc;

sealed class OidcException : ApplicationException
{
    public OidcException(ErrorResponse errorResponse)
    {
        ErrorResponse = errorResponse;
    }

    public ErrorResponse ErrorResponse { get; }
}