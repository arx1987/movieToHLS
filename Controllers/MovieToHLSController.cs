using Microsoft.AspNetCore.Mvc;
using MonoTorrent.Client;
using MovieToHLS.Services;

namespace MovieToHLS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MovieToHLSController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public MovieToHLSController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpPost("Upload")]
    //public IActionResult Upload()
    public async Task Upload()
    {
        var response = HttpContext.Response;
        var request = HttpContext.Request;
        response.ContentType = "text/html; charset=utf-8";
        IFormFileCollection files = request.Form.Files;
        //путь к папке, где будут храниться файлы
        var uploadPath = $"{Directory.GetCurrentDirectory()}/uploads";
        // создаем папку для хранения файлов
        Directory.CreateDirectory(uploadPath);

        foreach (var file in files)
        {
            // путь к папке uploads
            string fullPath = $"{uploadPath}/{file.FileName}";

            // сохраняем файл в папку uploads
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }
        await response.WriteAsync("Файлы успешно загружены");

        string[] allFiles = Directory.GetFiles(uploadPath);
        //var engine = new ClientEngine();
        //var loader = new TorrentService(engine);
        
        //return View();
    }
}
