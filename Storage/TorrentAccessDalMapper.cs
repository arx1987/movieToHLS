using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MovieToHLS.Storage;

public class TorrentAccessDalMapper : IEntityTypeConfiguration<Entities.TorrentAccess>
{
    public void Configure(EntityTypeBuilder<Entities.TorrentAccess> builder)//id, userId, torrentId
    {
        builder.ToTable("torrent_access");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.TorrentId).HasColumnName("torrent_id");
    }
}