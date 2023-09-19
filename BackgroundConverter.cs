using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Options;
using MovieToHLS.Entities;
using MovieToHLS.Services;
using Telegram.Bot;
using Torrent = MonoTorrent.Torrent;

namespace MovieToHLS;

public record ConvertVideoCmd(DirectoryInfo FolderWithFiles, long ChatId, User User, Torrent Torrent);

public interface IVideoConverter
{
    void PostCommand(ConvertVideoCmd cmd);
}

public class BackgroundVideoConverter : IHostedService, IVideoConverter
{
    private readonly ILogger<BackgroundVideoConverter> _logger;
    private readonly TelegramOptions _tgOptions;
    private readonly TelegramBotClient _tg;
    private readonly Store _store;
    private readonly IServiceScope _scope;
    private readonly Channel<ConvertVideoCmd> _channel;

    public BackgroundVideoConverter(
        IServiceProvider serviceProvider,
        ILogger<BackgroundVideoConverter> logger,
        IOptions<TelegramOptions> tgOptions,
        TelegramBotClient tg)
    {
        _logger = logger;
        _tgOptions = tgOptions.Value;
        _tg = tg;
        _channel = Channel.CreateUnbounded<ConvertVideoCmd>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
        _scope = serviceProvider.CreateScope();
        _store = _scope.ServiceProvider.GetRequiredService<Store>();
    }

    public void PostCommand(ConvertVideoCmd cmd) => _channel.Writer.TryWrite(cmd);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                var msg = await _channel.Reader.ReadAsync();
                try
                {
                    await HandleCmd(msg);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while convert video");
                }
            }
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    private async Task HandleCmd(ConvertVideoCmd arg)
    {
        await _tg.SendTextMessageAsync(arg.ChatId, "Вот ваше кино http://localhost:5000/api/MovieToHLS/download/filename");
        _logger.LogInformation("Download completed, converting to hls...");

        // var oldTorrentsDir = Directory.CreateDirectory(Path.Combine(uploadDir.FullName, "OldTorrentFilesDownloaded"));
        // string whereFileWillBe = Path.Combine(oldTorrentsDir.FullName, arg.Torrent.Name);//x.FileInfo.Name);
        // FileInfo torrentFileToMove = new(Path.Combine(uploadDir.FullName, torrentName));//x.FileInfo.Name));
        // if (System.IO.File.Exists(torrentFileToMove.FullName) && oldTorrentsDir.Exists)
        // {
        //     torrentFileToMove.MoveTo(whereFileWillBe, true);
        // }
        //
        // var allowedExt = new[] { ".mp4", ".avi", ".mkv", ".mov" };
        // var foldWIthFilesArray = folderWithFiles.EnumerateFiles("", SearchOption.AllDirectories)
        //          .Where(x => allowedExt.Contains(x.Extension)).ToArray();
        // var composite = foldWIthFilesArray.Length > 1;
        //
        // var videoGuid = Guid.NewGuid();
        // var videoDirectory = baseDir.CreateSubdirectory("video");
        // var folderVideoGuid = videoDirectory.CreateSubdirectory(videoGuid.ToString());
        //
        //
        // DirectoryInfo convertedDir = new(Path.Combine(videoDirectory.FullName, folderVideoGuid.Name));//torrent.Name, torrent.Name + "Converted"));
        // //if (!convertedDir.Exists) convertedDir.Create();
        // foreach (var videoFile in foldWIthFilesArray)
        // {
        //     var convertedFiles = FFmpegHelper.RunMyProcess(videoFile, convertedDir, fileName: folderVideoGuid.Name);// torrent.Name);//mb u should add videoGuidString instead of 3d prm
        //     _logger.LogInformation("Converting complete, {Count} files...", convertedFiles.Length);
        //     var bdTorrent = await _store.SaveTorrent(videoGuid, torrentName, torrent.InfoHash.ToArray());
        //     var torrentAccessId = await _store.SaveTorrentAccess(user, bdTorrent);
        //     /*закидываем в GoogleStorage*/
        //     //using var fileStream = videoFile.OpenRead();
        //     //await _fileStorage.UploadFile(fileStream, "....");//доделать здесь!!!
        //     await _tg.SendTextMessageAsync(arg.ChatId, $"Вот ваше кино \n{_tgOptions.HostUrl}/api/MovieToHLS/video/{torrentAccessId}"); //{torrent.Name.Replace(" ", "%20")}");
        // }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}