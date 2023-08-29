using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MovieToHLS.Storage;

public class TorrentDalMapper : IEntityTypeConfiguration<Entities.Torrent>
{
    public void Configure(EntityTypeBuilder<Entities.Torrent> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Slug).HasColumnName("slug");
        builder.Property(x => x.Title).HasColumnName("title");
    }
}
