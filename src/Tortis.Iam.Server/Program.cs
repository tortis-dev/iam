using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Identity;
using Tortis.Iam.Server;
using Tortis.Iam.Server.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("BoundedContextConnection") ?? throw new InvalidOperationException("Connection string 'BoundedContextConnection' not found.");

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages(options =>
{
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {

    });
builder.Services.AddAuthorization();

builder.Services.AddDbContext<BoundedContext>(options =>
{
    options.UseInMemoryDatabase("iam");
    
    //Add OpenIdDict models to the context
    options.UseOpenIddict();
});

// ASP.Net Identity without UI
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<BoundedContext>(); // <== Add Identity models to the context

// ASP.Net Identity with UI
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<BoundedContext>();

builder.AddOpenIdConnect();

var app = builder.Build();

await Seed(app.Services);

app.UseHttpsRedirection();
app.UseHsts();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();

// ====================================================================================================================
async Task Seed(IServiceProvider container)
{
    await using var scope = container.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<BoundedContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    await manager.CreateAsync(new OpenIddictApplicationDescriptor
    {
        ClientId = "foo",
        ClientSecret = "bar",
        DisplayName = "Foo",
        Permissions =
        {
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.Prefixes.Scope + "api",
            OnBehalfOfFlow.Permission
        }
    });

    var scopes = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
    await scopes.CreateAsync(new OpenIddictScopeDescriptor
    {
        Name = "api"
    });
}