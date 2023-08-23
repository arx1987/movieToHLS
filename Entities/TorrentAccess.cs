namespace MovieToHLS.Entities;

public class TorrentAccess
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public Guid TorrentId { get; }


    public User User { get; }
    public Torrent Torrent { get; }

    public TorrentAccess(Guid id, Guid userId, Guid torrentId)
    {
        Id = id;
        UserId = userId;
        TorrentId = torrentId;
    }
}

public class Torrent
{
    public Guid Id { get; }
    public string Slug { get; }
    public string Title { get; }

    public Torrent(Guid id, string slug, string title)
    {
        Id = id;
        Slug = slug;
        Title = title;
    }
}