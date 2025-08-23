using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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
            options.BackchannelHttpHandler = new HttpClientHandler(){ ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator};
        
        options.Authority = "https://localhost:5000";
        options.ClientId = "test";
        options.ClientSecret = "secret";
        
        // Tortis IAM uses a cookie to store the user's access and refresh tokens.
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        
        // We want to return an authorization code and an ID token.
        options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

        // Gets us the email claim in the access token.
        options.Scope.Add("email");
        
        // Gets us the an id_token
        options.Scope.Add("profile");
        
        // Gets us a refresh token.
        options.Scope.Add("offline_access");
        
        // Application specific scopes.
        options.Scope.Add("fullaccess");
        
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = "roles";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();