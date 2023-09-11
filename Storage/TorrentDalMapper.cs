using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MovieToHLS.Storage;

public class TorrentDalMapper : IEntityTypeConfiguration<Entities.Torrent>
{
    public void Configure(EntityTypeBuilder<Entities.Torrent> builder)
    {
        builder.ToTable("torrents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.VideoGuid).HasColumnName("video_guid");
        builder.Property(x => x.Title).HasColumnName("title");
        builder.Property(x => x.InfoHash).HasColumnName("info_hash");
    }
}
