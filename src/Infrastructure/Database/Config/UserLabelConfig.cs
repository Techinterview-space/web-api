using Domain.Entities.Labels;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class UserLabelConfig : IEntityTypeConfiguration<UserLabel>
{
    public void Configure(EntityTypeBuilder<UserLabel> builder)
    {
        builder.ToTable("UserLabels");
        builder
            .Property(x => x.HexColor)
            .HasConversion(
                x => x.ToString(),
                x => new HexColor(x));

        builder
            .HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById);
    }
}