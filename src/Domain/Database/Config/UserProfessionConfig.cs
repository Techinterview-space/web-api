using System.Linq;
using Domain.Entities.Salaries;
using Domain.Extensions;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Database.Config;

public class UserProfessionConfig : IEntityTypeConfiguration<UserProfession>
{
    public void Configure(EntityTypeBuilder<UserProfession> builder)
    {
        builder.ToTable("UserProfessions");

        builder
            .Property(x => x.HexColor)
            .HasConversion(
                x => x.ToString(),
                x => new HexColor(x));

        builder
            .HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById);

        builder.HasData(
            EnumHelper
                .Values<UserProfessionEnum>()
                .Where(x => x != UserProfessionEnum.Undefined)
                .Select(x => new UserProfession(x.ToString())));
    }
}