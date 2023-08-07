using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using MonoTorrent;
using MonoTorrent.Client;
using MovieToHLS.Services;
using System.Net;

namespace MovieToHLS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MovieToHLSController : ControllerBase
{
    private readonly ILogger<MovieToHLSController> _logger;
    private readonly TorrentService _service;

    public MovieToHLSController(ILogger<MovieToHLSController> logger, TorrentService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost("Upload")]
    
    public async Task Upload()
    {
        var response = HttpContext.Response;
        var request = HttpContext.Request;
        response.ContentType = "text/html; charset=utf-8";
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
        await response.WriteAsync("Файлы успешно загружены");

        var downloadedFiles = uploadDir.GetFiles()
            .Select(x => (IsValid: Torrent.TryLoad(x.FullName, out var torrent), torrent, fileInfo: x))//item3 - FileInfo
            .Where(x => x.IsValid)
            .Select(x => x);

        var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
        var outputDirectory = baseDir.CreateSubdirectory("Output");

        if (downloadedFiles.Any())
        {
            /*var engine = new ClientEngine();
            var loader = new TorrentService(engine);*/
            var allowedExt = new[] { ".mp4", ".avi", ".mkv", ".mov" };
            foreach (var (IsValid, torrent, fileInfo) in downloadedFiles)
            {
                _logger.LogInformation("Found torrent {Name}, downloading files...", torrent.Name) ;
                var folderWithFiles = _service.DownloadFile(torrent, outputDirectory);
                _logger.LogInformation("Download completed, converting to hls...");
               
                var oldTorrentsDir = Directory.CreateDirectory(Path.Combine(uploadDir.FullName, "OldTorrentFilesDownloaded"));
                string whereFileWillBe = Path.Combine(oldTorrentsDir.FullName, fileInfo.Name);
                FileInfo torrentFileToMove = new(Path.Combine(uploadDir.FullName, fileInfo.Name));
                if(System.IO.File.Exists(torrentFileToMove.FullName)&&oldTorrentsDir.Exists)
                {
                    torrentFileToMove.MoveTo(whereFileWillBe);
                }

                var foldWIthFilesArray = folderWithFiles.EnumerateFiles("", SearchOption.AllDirectories)
                         .Where(x => allowedExt.Contains(x.Extension)).ToArray();
                var composite = foldWIthFilesArray.Length > 1;

                DirectoryInfo convertedDir = new(Path.Combine(outputDirectory.FullName, torrent.Name, torrent.Name + "Converted"));
                if (!convertedDir.Exists) convertedDir.Create();
                foreach (var videoFile in foldWIthFilesArray)
                {
                    var convertedFiles = FFmpegHelper.RunMyProcess(videoFile, convertedDir);
                    var m3u8File = convertedFiles.First(s => s.Extension == ".m3u8");
                    _logger.LogInformation("Converting complete, upload to cloud {Count} files...", convertedFiles.Length);
                    //var uploadedFiles = cloudStorage.UploadDirectory(convertedDir.FullName, convertedFiles, torrent.Name, composite);
                    //var uploadedm3u8 = uploadedFiles.First(x => x.ObjectName.EndsWith(m3u8File.Name));
                    //Console.WriteLine($"https://players.akamai.com/players/hlsjs?streamUrl={WebUtility.UrlEncode(uploadedm3u8.PublicLink)}");
                }
            }
        }        
    }
}
