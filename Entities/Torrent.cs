namespace MovieToHLS.Entities;

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