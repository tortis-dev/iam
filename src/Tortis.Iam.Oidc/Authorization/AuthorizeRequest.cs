using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Tortis.Iam.Oidc.Authorization;

[ModelBinder(typeof(AuthorizeRequestBinder))]
public class AuthorizeRequest
{
    public string? ResponseType { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? RedirectUri { get; set; }
    public string? State { get; set; }
    public Dictionary<string, StringValues> Extensions { get; } = new();
}