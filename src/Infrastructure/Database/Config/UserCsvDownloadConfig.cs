using Domain.Entities.CSV;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config;

internal class UserCsvDownloadConfig : IEntityTypeConfiguration<UserCsvDownload>
{
    public void Configure(
        EntityTypeBuilder<UserCsvDownload> builder)
    {
        builder.ToTable("UserCsvDownloads");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.UserId)
            .IsRequired(true);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}