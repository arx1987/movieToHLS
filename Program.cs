using Microsoft.Extensions.Options;
using MovieToHLS.Services;
using MonoTorrent.Client;
using MovieToHLS;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var config = builder.Configuration;

services.AddControllers().AddNewtonsoftJson();
//services.AddSingleton<ITorrentService, TorrentService>();
services
    .AddSingleton<TelegramBotClient>(new TelegramBotClient(config.GetSection("TelegramOptions:Token").Get<string>()))
    .AddTransient<TelegramService>()
    .AddTransient<TorrentService>()
    .AddSingleton<ClientEngine>()
    .AddCors()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();


services.Configure<TelegramOptions>(config.GetSection(TelegramOptions.OptionName));

var app = builder.Build();

var tg = app.Services.GetRequiredService<TelegramBotClient>();
var tgOptions = app.Services.GetRequiredService<IOptions<TelegramOptions>>().Value;

await tg.SetWebhookAsync($"{tgOptions.HostUrl}/tg/webhook");


app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.MapGet("/", () => "Hello World!");

/*app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    response.ContentType = "text/html; charset=utf-8";
    if (request.Path == "/upload" && request.Method == "POST")
    {
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

        //string jobId = BackgroundJob.Enqueue(() => );
        //BackgroundJob.CountinueJobWith(jobId, () => );
    }
    else
    {
        await response.SendFileAsync("html/index.html");
    }
});*/

app.Run();