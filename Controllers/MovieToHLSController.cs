using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using MonoTorrent;
using MonoTorrent.Client;
using MovieToHLS.Services;
using System.Net;
using System.Web;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MovieToHLS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MovieToHLSController : ControllerBase
{
    private readonly ILogger<MovieToHLSController> _logger;
    private readonly TorrentService _service;
    private readonly TelegramService _telegram;
    private readonly TelegramBotClient _tg;
    private readonly TelegramOptions _tgOptions;

    public MovieToHLSController(
        ILogger<MovieToHLSController> logger,
        TorrentService service,
        TelegramService telegram,
        TelegramBotClient tg,
        IOptions<TelegramOptions> tgOptions)
    {
        _logger = logger;
        _service = service;
        //service.OnDownload += Service_OnDownload;
        _telegram = telegram;
        _tg = tg;
        _tgOptions = tgOptions.Value;
    }

    [HttpGet("/tg/webhook")]
    public void WebhookGet() {}


    [HttpPost("/tg/webhook")]
    public async Task Webhook([FromBody]Update update)
    {
        var chatId = update.Message.Chat.Id;
        if (update.Message?.Text is "/start" or "/help")
        {
            await _tg.SendTextMessageAsync(update.Message.Chat.Id, "Я умею конвертировать видосы, скинь мне торрент или напиши /help");
            return;
        }

        if (update.Message?.Document is not null)
        {
            var fileId = update.Message.Document.FileId;
            await using var stream = System.IO.File.Create(Path.Combine(AppContext.BaseDirectory, "uploads", $"{Guid.NewGuid():n}.torrent"));
            await _tg.GetInfoAndDownloadFileAsync(fileId, stream);

            stream.Position = 0;
            var torrent = await Torrent.LoadAsync(stream);

            // await _service.DownloadFile(torrent, null, async folderWithFiles =>
            // {
            //     await _tg.SendTextMessageAsync(chatId, "Вот ваше кино http://localhost:5000/api/MovieToHLS/download/filename");
            // });

            await _tg.SendTextMessageAsync(chatId, "Окей, твой торрент качается, скоро пришлю ссылку");
            await Task.Delay(1000);
            await _tg.SendTextMessageAsync(chatId, $"Вот ваше кино \n{_tgOptions.HostUrl}/api/MovieToHLS/download/filename");
            return;
        }

        await _tg.SendTextMessageAsync(chatId, "Я вас не понял, попробуйте /help");

        // ...
        // ...
    }


    [HttpPost("Upload")]

    public async Task<string> Upload()
    {
        /*_service.OnDownload += (a, b) => { };
        Action<DateTime, DirectoryInfo> some = (a, b) => { };
        _service.OnDownload += Service_OnDownload;
        _service.OnDownload += some;
        _service.OnDownload -= Service_OnDownload;
        _service.OnDownload -= some;*/
        var response = HttpContext.Response;
        var request = HttpContext.Request;
        IFormFileCollection files = request.Form.Files;
        //путь к папке, где будут храниться файлы
        DirectoryInfo uploadDir = new(Path.Combine(Directory.GetCurrentDirectory(), "uploads"));
        // создаем папку для хранения файлов
        Directory.CreateDirectory(uploadDir.FullName);


        foreach (var file in files)//file.filename  = "big-buck-bunny.torrent";
        {
            string fullPath = Path.Combine(uploadDir.FullName, file.FileName);
            // сохраняем файл в папку uploads
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }

        var downloadedFiles = uploadDir.GetFiles()
            .Select(x => new { IsValid = Torrent.TryLoad(x.FullName, out var torrent), Torrent = torrent, FileInfo = x })
            .Where(x => x.IsValid)
            .Select(x => x);

        var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
        var outputDirectory = baseDir.CreateSubdirectory("Output");

        if (!downloadedFiles.Any())
        {
            HttpContext.Response.StatusCode = 500;
            return "Не удалось скачать файл(ы)";
        }
        /*var engine = new ClientEngine();
        var loader = new TorrentService(engine);*/
        var allowedExt = new[] { ".mp4", ".avi", ".mkv", ".mov" };
        foreach (var x in downloadedFiles)
        {
            _logger.LogInformation("Found torrent {Name}, downloading files...", x.Torrent.Name);
            await _service.DownloadFile(x.Torrent, outputDirectory, async (folderWithFiles) =>
            {
                _logger.LogInformation("Download completed, converting to hls...");

                var oldTorrentsDir = Directory.CreateDirectory(Path.Combine(uploadDir.FullName, "OldTorrentFilesDownloaded"));
                string whereFileWillBe = Path.Combine(oldTorrentsDir.FullName, x.FileInfo.Name);
                FileInfo torrentFileToMove = new(Path.Combine(uploadDir.FullName, x.FileInfo.Name));
                if (System.IO.File.Exists(torrentFileToMove.FullName) && oldTorrentsDir.Exists)
                {
                    torrentFileToMove.MoveTo(whereFileWillBe, true);
                }

                var foldWIthFilesArray = folderWithFiles.EnumerateFiles("", SearchOption.AllDirectories)
                         .Where(x => allowedExt.Contains(x.Extension)).ToArray();
                var composite = foldWIthFilesArray.Length > 1;

                DirectoryInfo convertedDir = new(Path.Combine(outputDirectory.FullName, x.Torrent.Name, x.Torrent.Name + "Converted"));
                if (!convertedDir.Exists) convertedDir.Create();
                foreach (var videoFile in foldWIthFilesArray)
                {
                    var convertedFiles = FFmpegHelper.RunMyProcess(videoFile, convertedDir, x.Torrent.Name);
                    var m3u8File = convertedFiles.First(s => s.Extension == ".m3u8");
                    _logger.LogInformation("Converting complete, upload to cloud {Count} files...", convertedFiles.Length);
                    //[Link text Here] (https://link-url-here.org)
                    var message1 = "[Ваш фильм скачан: ]";
                    var message2 = $"(http://localhost:5000/api/movieToHLS/download/{m3u8File.Name.Replace(".m3u8", "")}?password=123)".Replace(" ", "%20");
                    /*var keyboard = new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(text: m3u8File.Name, url: message2));//HttpUtility.UrlEncode(message2)));*/
                    //await _telegram.Notify(message1 + HttpUtility.UrlEncode(message2));
                    await _telegram.Notify(message1 + message2); // keyboard);
                    /*var keyboard = new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl("Talk to me in private", "https://t.me/username"));
                    await Bot.SendTextMessageAsync(message.Chat, "Smth", replyMarkup: keyboard);*/

                    //var uploadedFiles = cloudStorage.UploadDirectory(convertedDir.FullName, convertedFiles, torrent.Name, composite);
                    //var uploadedm3u8 = uploadedFiles.First(x => x.ObjectName.EndsWith(m3u8File.Name));
                    //Console.WriteLine($"https://players.akamai.com/players/hlsjs?streamUrl={WebUtility.UrlEncode(uploadedm3u8.PublicLink)}");
                }
            });
        }
        return "Файлы добавлены на скачивание";
    }

    [HttpGet]
    [Route("download/{fileName}")]
    //[Route("/download/{fileName}")]
    public async Task<IResult> ReadFile([FromRoute] string fileName)//[FromQuery] string password
    {
        //if (password != "123") return Results.Unauthorized();

        DirectoryInfo uploadDir = new(Path.Combine(AppContext.BaseDirectory, "output"));
        if (fileName.EndsWith(".ts"))
        {
            var tsFileToGive = uploadDir.GetFiles(fileName, SearchOption.AllDirectories);
            return Results.File(tsFileToGive[0].FullName);
        }

        var m3u8FileToGive = uploadDir.GetFiles(fileName + ".m3u8", SearchOption.AllDirectories);

        if (m3u8FileToGive.Length < 1) return Results.BadRequest();//что-то другое нужно бы вернуть
        //await HttpContext.Response.SendFileAsync(m3u8FileToGive[0].FullName);

        return Results.File(m3u8FileToGive[0].FullName);

    }

    [HttpGet]
    [Route("test100500/{fileName}")]
    public async Task<string> Test([FromQuery] string password, [FromRoute] string fileName)
    {
        return "fileName: " + fileName + ", password: " + password;
    }

}