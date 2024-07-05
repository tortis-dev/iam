using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tortis.Iam.Oidc.Authorization.AuthorizationCodeGrantFlow;
using Tortis.Iam.Oidc.Clients;

namespace Tortis.Iam.Oidc.EntityFramework;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AddTortisIamModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tortis_iam");
        var client = modelBuilder.Entity<Client>().ToTable("clients");
        client.HasKey(c => c.ClientId).HasName("pk_clients");
        client.Property(c => c.ClientId).HasColumnName("client_id").HasMaxLength(255);
        client.Property(c => c.ClientSecret).HasColumnName("client_secret").HasMaxLength(int.MaxValue);
        client.Property(c => c.RedirectUris).HasColumnName("redirect_uris")
            .HasConversion<HashsetConverter<string>>()
            .HasMaxLength(int.MaxValue);

        var code = modelBuilder.Entity<IssuedCode>().ToTable("issued_codes");
        code.HasKey(c => c.Code).HasName("pk_issued_codes");
        code.Property(c => c.Code).HasColumnName("code").HasMaxLength(64);
        code.Property(c => c.ClientId).HasColumnName("client_id").HasMaxLength(255);
        code.Property(c => c.ClientId).HasColumnName("redirect_uri").HasMaxLength(2048);
        return modelBuilder;
    }
}

class HashsetConverter<T> : ValueConverter<HashSet<T>?, string>
{
    // ReSharper disable once StaticMemberInGenericType
    static readonly JsonSerializerOptions _options = new JsonSerializerOptions();
    
    public HashsetConverter()
        : base (dto => JsonSerializer.Serialize(dto, _options),
            dao => JsonSerializer.Deserialize<HashSet<T>>(dao, _options))
    { }
}