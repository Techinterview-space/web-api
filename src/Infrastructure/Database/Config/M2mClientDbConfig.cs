using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

internal class M2mClientDbConfig :
    IEntityTypeConfiguration<M2mClient>,
    IEntityTypeConfiguration<M2mClientScope>
{
    public void Configure(EntityTypeBuilder<M2mClient> builder)
    {
        builder.HasIndex(x => x.ClientId).IsUnique();

        builder.HasIndex(x => x.Name);

        builder
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasMaxLength(1000);

        builder
            .Property(x => x.ClientId)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(x => x.ClientSecretHash)
            .HasMaxLength(200);

        builder
            .Property(x => x.LastUsedIpAddress)
            .HasMaxLength(50);
    }

    public void Configure(EntityTypeBuilder<M2mClientScope> builder)
    {
        builder.HasIndex(x => new { x.M2mClientId, x.Scope }).IsUnique();

        builder
            .HasOne(x => x.M2mClient)
            .WithMany(x => x.Scopes)
            .HasForeignKey(x => x.M2mClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.Scope)
            .HasMaxLength(100)
            .IsRequired();
    }
}
