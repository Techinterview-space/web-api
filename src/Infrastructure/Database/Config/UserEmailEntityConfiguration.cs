using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

public class UserEmailEntityConfiguration : IEntityTypeConfiguration<UserEmail>
{
    public void Configure(
        EntityTypeBuilder<UserEmail> builder)
    {
        builder.HasKey(x => x.Id);
        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Emails)
            .HasForeignKey(x => x.UserId);
    }
}