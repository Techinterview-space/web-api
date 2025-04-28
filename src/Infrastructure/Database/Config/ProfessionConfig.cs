using Domain.Entities.Salaries;
using Domain.Extensions;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class ProfessionConfig : IEntityTypeConfiguration<Profession>
{
    public void Configure(EntityTypeBuilder<Profession> builder)
    {
        builder.ToTable("Professions");

        builder
            .Property(x => x.HexColor)
            .HasConversion(
                x => x.ToString(),
                x => new HexColor(x));

        builder
            .HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById);

        var creationDate = new DateTime(2024, 1, 1);
        builder.HasData(
            EnumHelper
                .Values<UserProfessionEnum>()
                .Where(x => x != UserProfessionEnum.Undefined)
                .Select(x => new Profession((long)x, x.ToString(), new HexColor("#C00C50"), creationDate)));
    }
}