using Domain.Entities.StatData.Salary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class JobPostingMessageSubscriptionEntityConfig : IEntityTypeConfiguration<JobPostingMessageSubscription>
{
    public void Configure(
        EntityTypeBuilder<JobPostingMessageSubscription> builder)
    {
        builder.ToTable("JobPostingMessageSubscriptions");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .IsRequired();

        builder
            .Property(x => x.TelegramChatId)
            .IsRequired();

        builder
            .Property(x => x.ProfessionIds)
            .HasJsonConversion();

        builder
            .HasIndex(x => x.TelegramChatId)
            .IsUnique();
    }
}