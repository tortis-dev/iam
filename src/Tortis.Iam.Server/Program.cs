using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tortis.Iam.Oidc;
using Tortis.Iam.Oidc.EntityFramework;
using Tortis.Iam.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tortis.Iam.Oidc.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.AddTortisIamOidc();

builder.Services.AddDbContext<BoundedContext>(options =>
{
    options.UseInMemoryDatabase("iam");
}).AddTortisIamEntityFramework<BoundedContext>();

//TODO: Support Cookie
builder.Services.AddAuthentication()
    .AddBasic()
    .AddCookie();
builder.Services.AddAuthorization();

var app = builder.Build();

// https://datatracker.ietf.org/doc/html/rfc6749#section-2.3.1
// The authorization server MUST require the use of TLS as described in
// Section 1.6 when sending requests using password authentication.
app.UseHsts();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();



app.Run();



public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder,
        Action<BasicAuthenticationOptions>? options = null)
    {
        builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>("Basic", options ?? (_ => { }));
        return builder;
    }
}

public class BasicAuthenticationOptions : AuthenticationSchemeOptions
{
    
}

class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
    readonly ClientService _clientService;

    public BasicAuthenticationHandler(IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ClientService clientService) : base(options, logger, encoder, clock)
    {
        _clientService = clientService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        //https://datatracker.ietf.org/doc/html/rfc6749#section-2.3.1
        // The authorization server MUST support the HTTP Basic authentication scheme for authenticating
        // clients that were issued a client password.
        var authHeader = Request.Headers.Authorization.ToString().Split(' ');
        
        if (authHeader.Length <= 0 || authHeader[0] != "Basic") 
            return AuthenticateResult.NoResult();
        
        //TODO: The client MUST NOT use more than one authentication method in each request.
            
        var decodedBasicAuthentication = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(authHeader[1])).Split(':');
        var clientId = decodedBasicAuthentication[0];
        var clientSecret = decodedBasicAuthentication[1];

        var client = await _clientService.FindByClientIdAsync(clientId, CancellationToken.None);

        if (client != null && client.ClientId == clientId && clientSecret == client.ClientSecret)
        {
            var nameClaim = new Claim(ClaimTypes.Name, "Foo");
            var identity = new ClaimsIdentity(new[] { nameClaim }, nameof(BasicAuthenticationHandler));
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Basic"));
        }

        return AuthenticateResult.Fail("Unauthorized");


    }
}