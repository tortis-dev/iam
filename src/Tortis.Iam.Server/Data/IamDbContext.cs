using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Tortis.Iam.Server.Data;

public class IamDbContext : IdentityDbContext<IamUser, IdentityRole<Guid>, Guid>
{
    public IamDbContext(DbContextOptions<IamDbContext> options) : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        if (base.Database.IsSqlServer())
            builder.HasDefaultSchema("iam");

        // Identity Tables
        builder.Entity<IamUser>(user =>
        {
            user.ToTable("iam_users");
            user.Ignore(p => p.AccountLocked);
            user.HasIndex(p => p.NormalizedEmail).HasDatabaseName("ix_iam_users_email");
            user.HasIndex(p => p.NormalizedUserName).HasDatabaseName("ix_iam_users_username");
        });
        builder.Entity<IdentityRole<Guid>>(role =>
        {
            role.ToTable("iam_roles");
            role.HasIndex(p => p.NormalizedName).HasDatabaseName("ix_iam_role_name");
        });
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("iam_role_claim");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("iam_user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("iam_user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("iam_user_login");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("iam_user_token");
        
        //OpenIdDict Tables
        builder.Entity<OpenIddictEntityFrameworkCoreApplication<Guid>>().ToTable("iam_oidc_applications");
        builder.Entity<OpenIddictEntityFrameworkCoreScope<Guid>>().ToTable("iam_oidc_scopes");
        builder.Entity<OpenIddictEntityFrameworkCoreAuthorization<Guid>>().ToTable("iam_oidc_authorizations");
        builder.Entity<OpenIddictEntityFrameworkCoreToken<Guid>>().ToTable("iam_oidc_tokens");
    }
}