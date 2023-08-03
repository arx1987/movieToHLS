using MonoTorrent;
using MonoTorrent.Client;

namespace MovieToHLS.Services;

/*public interface ITorrentService
{

}
*/
public class TorrentService //: ITorrentService
{
    private Semaphore _isDownloaded;
    private readonly ClientEngine _engine;

    public TorrentService(ClientEngine engine)
    {
        _engine = engine;
        _isDownloaded = new Semaphore(0, 1);
    }

    public DirectoryInfo DownloadFile(Torrent torrent, DirectoryInfo downloadDir)
    {
        var manager = _engine.AddAsync(torrent, downloadDir.FullName).Result;
        manager.StartAsync().Wait();
        manager.TorrentStateChanged += StateChanged;

        _isDownloaded.WaitOne();

        manager.TorrentStateChanged -= StateChanged;

        return new DirectoryInfo(Path.Combine(downloadDir.FullName, torrent.Name));
    }

    private void StateChanged(object? sender, TorrentStateChangedEventArgs e)
    {
        if (e.NewState != TorrentState.Seeding) return;

        _isDownloaded.Release();
    }
}
