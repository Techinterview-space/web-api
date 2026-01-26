using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

internal class UserDbConfig : IEntityTypeConfiguration<User>, IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasIndex(x => x.Email)
            .IsUnique();

        builder
            .HasIndex(x => x.IdentityId)
            .IsUnique();

        builder
            .Property(x => x.TotpSecret)
            .HasMaxLength(100);

        builder
            .HasIndex(x => x.UniqueToken)
            .IsUnique();

        builder
            .Property(x => x.PasswordHash)
            .HasMaxLength(200);

        builder
            .Property(x => x.PasswordResetToken)
            .HasMaxLength(100);

        builder
            .HasIndex(x => x.PasswordResetToken);

        builder
            .Property(x => x.EmailVerificationToken)
            .HasMaxLength(100);

        builder
            .HasIndex(x => x.EmailVerificationToken);
    }

    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(x => new { x.RoleId, x.UserId });
        builder
            .HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId);
    }
}