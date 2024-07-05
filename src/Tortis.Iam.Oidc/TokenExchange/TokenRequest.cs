﻿namespace Tortis.Iam.Oidc.TokenExchange;

public class TokenRequest
{
    public string? grant_type { get; set; }
    public string? response_type { get; set; }
    public string? client_id { get; set; }
    public string? client_secret { get; set; }
    public string? code { get; set; }
    public string? redirect_uri { get; set; }
}