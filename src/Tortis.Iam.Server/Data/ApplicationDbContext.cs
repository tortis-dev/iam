using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Tortis.Iam.Server.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        if (!base.Database.IsSqlite())
            builder.HasDefaultSchema("iam");
        
        var user = builder.Entity<ApplicationUser>().ToTable("iam_users");
        user.HasIndex(p => p.NormalizedEmail).HasDatabaseName("ix_iam_users_email");
        user.HasIndex(p => p.NormalizedUserName).HasDatabaseName("ix_iam_users_username");
        
        var role = builder.Entity<IdentityRole<Guid>>().ToTable("iam_roles");
        role.HasIndex(p => p.NormalizedName).HasDatabaseName("ix_iam_role_name");

        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("iam_role_claim");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("iam_user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("iam_user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("iam_user_login");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("iam_user_token");
    }
}