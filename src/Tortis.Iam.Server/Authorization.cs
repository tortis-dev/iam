// Licensed under GPL-3 (https://www.gnu.org/licenses/gpl-3.0.en.html)

using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;

using Tortis.Iam.Server.Components.Roles;
using Tortis.Iam.Server.Components.Users;

namespace Tortis.Iam.Server;

public static class Authorization
{
    public const string ADMINISTRATORS = "Administrators";
    public const string SECURITY_ADMINISTRATORS = "Security Administrators";
    public const string OIDC_ADMINISTRATORS = "OIDC Administrators";

    public static WebApplicationBuilder AddAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
            options.AddPolicy(ADMINISTRATORS, policy => policy.RequireRole(ADMINISTRATORS));
            options.AddPolicy(SECURITY_ADMINISTRATORS, policy => policy.RequireRole(ADMINISTRATORS, SECURITY_ADMINISTRATORS));
            options.AddPolicy(OIDC_ADMINISTRATORS,  policy => policy.RequireRole(ADMINISTRATORS, OIDC_ADMINISTRATORS));
        });
        
        return builder;
    }
}

class IamUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IamUser, IamRole>
{
    public IamUserClaimsPrincipalFactory(UserManager<IamUser> userManager, RoleManager<IamRole> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
    {
    }
    
    public override async Task<ClaimsPrincipal> CreateAsync(IamUser user)
    {
        var claimsPrincipal = await base.CreateAsync(user);

        if (!string.IsNullOrWhiteSpace(user.GivenName)) claimsPrincipal.AddClaim(ClaimTypes.GivenName, user.GivenName);
        if (!string.IsNullOrWhiteSpace(user.FamilyName)) claimsPrincipal.AddClaim(ClaimTypes.Surname, user.FamilyName);
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber)) claimsPrincipal.AddClaim(ClaimTypes.OtherPhone, user.PhoneNumber);
        
        return claimsPrincipal;
    }
}