using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment();

builder.Services.AddAuthentication(options =>
    {
        // This application will use a Cookie to store the user's access and refresh tokens
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        
        // To "login", users are challenged using OpenID Connect.
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(options =>
    {
        if (builder.Environment.IsDevelopment())
            options.BackchannelHttpHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        
        options.Authority = "https://localhost:5000";
        options.ClientId = "test";
        options.ClientSecret = "secret";
        
        // Tortis IAM uses a cookie to store the user's access and refresh tokens.
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        
        // We want to return an authorization code.
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        
       
        // Gets us an id_token
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("roles");

        // Gets us the email claim in the access token.
        options.Scope.Add("email");
        
        // Gets us a refresh token.
        options.Scope.Add("offline_access");
        
        // Application specific scopes.
        options.Scope.Add("fullaccess");
        
        options.SaveTokens = true;
        
        //Since code flow doesn't give us an id_token, we need to get the user's info from the endpoint
        options.GetClaimsFromUserInfoEndpoint = true;
        
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = "role";
        //options.ClaimActions.Add(new MapAllClaimsAction());

        // If we need to force the code challenge, we can do it here. AspNetCore is finicky with the HybridFlow.
        // options.Events.OnRedirectToIdentityProvider = context =>
        // {
        //     // Only add if not present
        //     if (!context.ProtocolMessage.Parameters.ContainsKey("code_challenge"))
        //     {
        //         // Generate a code_verifier (43-128 chars) and code_challenge (SHA256)
        //         var codeVerifier = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        //         codeVerifier = codeVerifier.Substring(0, 64);
        //
        //         using var sha256 = System.Security.Cryptography.SHA256.Create();
        //         var codeChallengeBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(codeVerifier));
        //         var codeChallenge = Base64UrlEncode(codeChallengeBytes);
        //
        //         context.ProtocolMessage.Parameters["code_challenge"] = codeChallenge;
        //         context.ProtocolMessage.Parameters["code_challenge_method"] = "S256";
        //         // Save verifier in correlation state or cookie for token exchange
        //         context.Properties.Items["code_verifier"] = codeVerifier;
        //     }
        //     return Task.CompletedTask;
        //
        //     // Helper for Base64UrlEncode
        //     static string Base64UrlEncode(byte[] arg)
        //     {
        //         return Convert.ToBase64String(arg)
        //             .TrimEnd('=')
        //             .Replace('+', '-')
        //             .Replace('/', '_');
        //     }
        // };
        
        // options.Events.OnAuthorizationCodeReceived = context =>
        // {
        //     // Add the code_verifier on the token request
        //     if (context.Properties.Items.TryGetValue("code_verifier", out var verifier))
        //         context.TokenEndpointRequest.Parameters["code_verifier"] = verifier;
        //
        //     return Task.CompletedTask;
        // };
        options.Events.OnTokenValidated = async context =>
        {
            /*
             * The OIDC middleware only populates the ClaimsPrincipal from claims in the ID token (and optionally /userinfo,
             * if you enable GetClaimsFromUserInfoEndpoint, and the server implements it).
               The access token is not consumed by the OIDC handler for claims population; 
               it is usually meant for API access, not for authentication/user info.
             */
            // Get the access token
            var accessToken = new JsonWebToken(context.TokenEndpointResponse.AccessToken);
            
            // Extract role claims from access token if present
            var roleClaims = accessToken?.Claims.Where(c => c.Type == "role");
            if (roleClaims != null)
            {
                var identity = (ClaimsIdentity)context.Principal.Identity;
                foreach (var roleClaim in roleClaims)
                {
                    identity.AddClaim(new Claim("role", roleClaim.Value));
                }
            }
            await Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();