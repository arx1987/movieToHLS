namespace MovieToHLS.Entities;

public class Torrent
{
    public Guid Id { get; }
    //public string Slug { get; }
    public Guid VideoGuid { get; }
    public string Title { get; }
    public byte[] InfoHash { get; }

    public Torrent(Guid id, Guid videoGuid, string title, byte[] infoHash)
    {
        Id = id;
        VideoGuid = videoGuid;
        Title = title;
        InfoHash = infoHash;
    }
}