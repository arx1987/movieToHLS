using Microsoft.EntityFrameworkCore;
//using MonoTorrent;
using MovieToHLS.Entities;
using MovieToHLS.Storage;
//using Telegram.Bot.Types;

namespace MovieToHLS.Services;

public class Store
{
    public readonly AppDBContext _dbContext;

    public Store(AppDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TorrentAccess?> GetAccessOrNull(Guid tokenId) => 
        await _dbContext.TorrentAccesses.FirstOrDefaultAsync(x => x.Id == tokenId);

    public async Task<TorrentAccess?> GetAccessOrNull(Guid userId, Guid tokenId) =>
        await _dbContext.TorrentAccesses.FirstOrDefaultAsync(x => x.Id == tokenId && x.UserId == userId);

    public async Task<Guid?> GetTorrentAccessIdOrNull(User user, byte[] InfoHash)
    {
        var torrent = await _dbContext.Torrents.FirstAsync(x => x.InfoHash == InfoHash);
        return await GetTorrentAccessIdOrNull(user, torrent);
    }

    public async Task<Guid?> GetTorrentAccessIdOrNull(User user, Entities.Torrent torrent)
    {
        var torrentAccess = await _dbContext.TorrentAccesses.FirstOrDefaultAsync(x => x.UserId == user.Id && x.TorrentId == torrent.Id);
        return torrentAccess?.Id;
    }

    public async Task<TorrentAccess> SaveTorrentAccess(string chatId, string titleName)
    {
        throw new NotImplementedException();
    }

    public async Task<Guid> SaveTorrentAccess(User user, Entities.Torrent torrent)
    {
        //без проверки - опасно? мы вызываем этот метод где уже проверка была, но.....
        var newTorrentAccessLine = new TorrentAccess(Guid.NewGuid(), user.Id, torrent.Id);
        await _dbContext.AddAsync(newTorrentAccessLine);
        await _dbContext.SaveChangesAsync();
        return newTorrentAccessLine.Id;
    }
    

    public async Task<User> SaveUser(string chatId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.ChatId == chatId);
        if (user == null)
        {
            user = new User(Guid.NewGuid(), chatId);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }
        return user;
    }

    public async Task<Torrent> SaveTorrent(Guid videoGuid, string title, byte[] infoHash)
    {
        var torrent = new Torrent(Guid.NewGuid(), videoGuid, title, infoHash);
        await _dbContext.Torrents.AddAsync(torrent);
        await _dbContext.SaveChangesAsync();
        return torrent;
    } 
 
    public async Task<Torrent?> GetTorrentOrNull(byte[] infoHash) =>
        await _dbContext.Torrents.FirstOrDefaultAsync(x => x.InfoHash == infoHash);

}