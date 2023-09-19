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
using System.Numerics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MovieToHLS.Entities;

namespace MovieToHLS.Controllers;


[Route("api/[controller]")]
[ApiController]
public class MovieToHLSController : ControllerBase
{
    private readonly ILogger<MovieToHLSController> _logger;
    private readonly TorrentService _service;
    private readonly TelegramService _telegram;
    private readonly TelegramBotClient _tg;
    private readonly Store _store;
    private readonly FileStorage _fileStorage;
    private readonly IMemoryCache _memoryCache;
    private readonly IVideoConverter _videoConverter;
    private readonly TelegramOptions _tgOptions;

    public MovieToHLSController(
        ILogger<MovieToHLSController> logger,
        TorrentService service,
        TelegramService telegram,
        TelegramBotClient tg,
        Store store,
        FileStorage fileStorage,
        IMemoryCache memoryCache,
        IVideoConverter videoConverter,
        IOptions<TelegramOptions> tgOptions)
    {
        _logger = logger;
        _service = service;
        _telegram = telegram;
        _tg = tg;
        _store = store;
        _fileStorage = fileStorage;
        _memoryCache = memoryCache;
        _videoConverter = videoConverter;
        _tgOptions = tgOptions.Value;
    }

    [HttpGet("/tg/webhook")]
    public void WebhookGet() { }

    [HttpPost("/tg/webhook")]
    public async Task Webhook([FromBody] Update update)
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
            //путь к папке, где будут храниться торренты
            var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
            var uploadDir = baseDir.CreateSubdirectory("uploads");
            //DirectoryInfo uploadDir = new(Path.Combine(Directory.GetCurrentDirectory(), "uploads"));
            // создаем папку для хранения файлов
            Directory.CreateDirectory(uploadDir.FullName);
            await using var stream = System.IO.File.Create(Path.Combine(uploadDir.FullName, $"{Guid.NewGuid():n}.torrent"));
            await _tg.GetInfoAndDownloadFileAsync(fileId, stream);

            stream.Position = 0;
            var torrent = await MonoTorrent.Torrent.LoadAsync(stream);
            //создаем юзера, если его нет в базе иначе ничего не делаем
            var user = await _store.SaveUser(chatId.ToString());
            //Как только мы получили торрент, мы должны проверить есть ли он в базе, чтобы не делать лишнего
            var bdTorrent = await _store.GetTorrentOrNull(torrent.InfoHash.ToArray() );
            if (bdTorrent != null){
                //проверить есть ли этот торрент у этого юзера, если у него нет
                //добавить запись в торрентАксес
                var torrentAccessIdOrNull = await _store.GetTorrentAccessIdOrNull(user, bdTorrent);
                if(torrentAccessIdOrNull == null) {
                    torrentAccessIdOrNull = await _store.SaveTorrentAccess(user, bdTorrent);
                }
                var torrentAccessId = torrentAccessIdOrNull;
                //и в любом случае отправить ссыль в телегу
                await _tg.SendTextMessageAsync(chatId, $"Вот ваше кино \n{_tgOptions.HostUrl}/video/{torrent}"); //{torrent.Name.Replace(" ", "%20")}");
                return;
            }
            //если торрента нет в базе, то качаем, конвертируем, сохраняем всю инфу о торренте и файлах
            var torrentName = stream.Name.Replace(uploadDir.FullName + "\\", "");

            var outputDirectory = baseDir.CreateSubdirectory("output");
            await _tg.SendTextMessageAsync(chatId, "Окей, твой торрент качается, скоро пришлю ссылку");
            await _service.DownloadFile(torrent, outputDirectory, async folderWithFiles =>
            {
                _videoConverter.PostCommand(new ConvertVideoCmd(folderWithFiles, chatId, user, torrent));
            });

            return;
        }

        await _tg.SendTextMessageAsync(chatId, "Я вас не понял, попробуйте /help");
    }

    // public record MyFile(Guid Id, string Name);
    //
    // //[Authorize]
    // [HttpGet("/api/get-my-files/{page}")]
    // public async Task<MyFile[]> GetFiles(int page)
    // {
    //     const int pageSize = 10;
    //     var userId = Guid.NewGuid(); //User.UserId();
    //
    //     var files = await _store._dbContext
    //         .TorrentAccesses
    //         .Where(x => x.UserId == userId)
    //         .OrderByDescending(x => x.Id)
    //         //.Join(_store._dbContext.Torrents, torrentAccess => torrentAccess.TorrentId, t => t.Id, (ta, torrent) => new MyFile(torrent.Id, torrent.Title))
    //         .Skip(pageSize * page)
    //         .Take(page)
    //         .Select(x => new MyFile(x.Torrent.Id, x.Torrent.Title))
    //         .ToArrayAsync();
    //
    //     return files;
    // }

    //[Authorize]
    // [HttpGet("/api/file-share/{torrentName}")]
    // public async Task<IResult> GetFileStream(string torrentName)
    // {
    //     var userId = Guid.NewGuid();//User.UserId();
    //     var split = torrentName.Split('.');
    //     var torrentId = Guid.Parse(split[0]);
    //     var torrent = await _store._dbContext
    //         .TorrentAccesses
    //         .Where(x => x.UserId == userId)
    //         .Where(x => x.TorrentId == torrentId)
    //         .Select(x => x.Torrent)
    //         .FirstOrDefaultAsync() ?? throw new ApplicationException("not found video");
    //
    //     return Results.File($"/upload/{torrent.VideoGuid}/{torrent.VideoGuid}.{split[1]}");
    // }

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
            return Results.File(tsFileToGive[0].FullName, fileDownloadName: fileName + ".ts");
        }

        var m3u8FileToGive = uploadDir.GetFiles(fileName + ".m3u8", SearchOption.AllDirectories);

        if (m3u8FileToGive.Length < 1) return Results.BadRequest();//что-то другое нужно бы вернуть
        //await HttpContext.Response.SendFileAsync(m3u8FileToGive[0].FullName);

        return Results.File(m3u8FileToGive[0].FullName, fileDownloadName: fileName+".m3u8");
    }

    [Authorize]
    [HttpGet("/video/{token}")]//{token} - it's JWT token
    public async Task GetVideo(Guid token)
    {
        var userId = User.UserId();
        var accessToken = await _memoryCache.GetOrCreateAsync($"access-token-{token}",
            async x =>
            {
                x.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                return await _store.GetAccessOrNull(userId, token) ?? throw new ApplicationException($"you dont have access to this video");
            });

        //using var stream = _fileStorage.GetMainFileBySlug(slug, 10);

        //return File()
        // accessToken.Torrent.Slug // /video/{slug}/{slug}.m3u8;
    }

}