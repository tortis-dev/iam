using Microsoft.EntityFrameworkCore;
using Tortis.Iam.Oidc.EntityFramework;

namespace Tortis.Iam.Server;

class BoundedContext : DbContext
{
    public BoundedContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tortis_iam");
        modelBuilder.AddTortisIamModel();
    }
}