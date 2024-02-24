using Domain.Entities.Salaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Database.Config;

public class UserSalaryConfig : IEntityTypeConfiguration<UserSalary>
{
    public void Configure(EntityTypeBuilder<UserSalary> builder)
    {
        builder.ToTable("UserSalaries");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Skill)
            .WithMany(x => x.Salaries)
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.WorkIndustry)
            .WithMany(x => x.Salaries)
            .HasForeignKey(x => x.WorkIndustryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Profession)
            .WithMany(x => x.Salaries)
            .HasForeignKey(x => x.ProfessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Property(x => x.UseInStats)
            .HasDefaultValue(true);
    }
}