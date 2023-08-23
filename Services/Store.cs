using MovieToHLS.Entities;

namespace MovieToHLS.Services;

public class Store
{
    public Store()
    {

    }

    public async Task<TorrentAccess> GetAccess(Guid tokenId)
    {
        throw new NotImplementedException();
    }

    public async Task<TorrentAccess> GetAccess(Guid userId, Guid tokenId)
    {
        throw new NotImplementedException();
    }

    public async Task<TorrentAccess> SaveTorrentAccess(string chatId, string torrentTitle)
    {
        throw new NotImplementedException();
    }
}