using Domain.Entities.Interviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Config
{
    internal class ShareLinkConfig : IEntityTypeConfiguration<ShareLink>
    {
        public void Configure(EntityTypeBuilder<ShareLink> builder)
        {
            builder.ToTable("ShareLinks");
            builder.HasKey(x => x.Id);
        }
    }
}