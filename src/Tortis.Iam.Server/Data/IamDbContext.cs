using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

using Tortis.Iam.Server.Components.Resources;
using Tortis.Iam.Server.Components.Roles;
using Tortis.Iam.Server.Components.Users;

namespace Tortis.Iam.Server.Data;

public class IamDbContext : IdentityDbContext<IamUser, IamRole, Guid>
{
    // TODO: Ultimately, we don't want to use EF Core migrations. We want to use a database migration tool. EF Core
    //       migrations are a pain when it comes to supporting multiple database providers.
    public const string HISTORY_TABLE_NAME = "iam_schema_migrations_history";
    
    public IamDbContext(DbContextOptions<IamDbContext> options) : base(options)
    { }

    public DbSet<IamResource> ApiResources { get; set; } = null!;
    
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
            user.Property(p => p.GivenName).HasMaxLength(100);
            user.Property(p => p.FamilyName).HasMaxLength(100);
            user.Property(p => p.CreatedOn);
            user.Property(p => p.CreatedBy).HasMaxLength(36);
            user.Property(p => p.ModifiedOn);
            user.Property(p => p.ModifiedBy).HasMaxLength(36);
            user.Ignore(p => p.AccountLocked);
            user.Ignore(p => p.Name);
            user.HasIndex(p => p.NormalizedEmail).HasDatabaseName("ix_iam_users_email");
            user.HasIndex(p => p.NormalizedUserName).HasDatabaseName("ix_iam_users_username");
        });
        builder.Entity<IamRole>(role =>
        {
            role.ToTable("iam_roles");
            role.Property(p => p.Description).HasMaxLength(512);
            role.Property(p => p.CreatedOn);
            role.Property(p => p.CreatedBy).HasMaxLength(36);
            role.Property(p => p.ModifiedOn);
            role.Property(p => p.ModifiedBy).HasMaxLength(36);
            role.HasIndex(p => p.NormalizedName).HasDatabaseName("ix_iam_role_name");
        });
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("iam_role_claim");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("iam_user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("iam_user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("iam_user_login");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("iam_user_token");
        
        //OpenIddict Tables
        builder.Entity<OpenIddictEntityFrameworkCoreApplication<Guid>>().ToTable("iam_oidc_applications");
        builder.Entity<OpenIddictEntityFrameworkCoreScope<Guid>>().ToTable("iam_oidc_scopes");
        builder.Entity<OpenIddictEntityFrameworkCoreAuthorization<Guid>>().ToTable("iam_oidc_authorizations");
        builder.Entity<OpenIddictEntityFrameworkCoreToken<Guid>>().ToTable("iam_oidc_tokens");
        
        //IAM Extension Tables
        builder.Entity<IamResource>(api =>
        {
            api.ToTable("iam_oidc_api_resources");
            api.HasKey(p => p.Id);
            api.Property(p => p.Id);
            api.Property(p => p.Urn).HasMaxLength(255);
            api.Property(p => p.Description).HasMaxLength(1024);
            api.Property(p => p.CreatedBy).HasMaxLength(36);
            api.Property(p => p.CreatedOn);
            api.Property(p => p.ModifiedBy).HasMaxLength(36);
            api.Property(p => p.ModifiedOn);
            api.Property(p => p.ConcurrencyToken).HasMaxLength(36).IsConcurrencyToken();
            api.HasIndex(p => p.Urn).HasDatabaseName("ix_iam_oidc_api_resources_audience");
        });
    }
}