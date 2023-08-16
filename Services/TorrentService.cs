using MonoTorrent;
using MonoTorrent.Client;

namespace MovieToHLS.Services;

/*public interface ITorrentService
{

}
*/
public class TorrentService //: ITorrentService
{
    //private SemaphoreSlim _isDownloaded;
    private readonly ClientEngine _engine;
    //public event Action<DateTime, DirectoryInfo> OnDownload;
    public TorrentService(ClientEngine engine)
    {
        _engine = engine;
        //_isDownloaded = new(0, 1);
    }

    public async Task DownloadFile(Torrent torrent, DirectoryInfo downloadDir, Func<DirectoryInfo, Task> onDownloaded)
    {
        var manager = await _engine.AddAsync(torrent, downloadDir.FullName);
        manager.TorrentStateChanged += StateChanged;
        await manager.StartAsync();

        async void StateChanged(object? sender, TorrentStateChangedEventArgs e)
        {
            if (e.NewState != TorrentState.Seeding) return;
            manager.TorrentStateChanged -= StateChanged;

            var downloadPath =  new DirectoryInfo(Path.Combine(downloadDir.FullName, torrent.Name));
            await onDownloaded(downloadPath);
            //OnDownload(DateTime.Now, downloadPath);
        }

    }

}
