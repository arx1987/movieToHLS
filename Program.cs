using MovieToHLS.Services;
using MonoTorrent.Client;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.AddControllers();
//services.AddSingleton<ITorrentService, TorrentService>();
services.AddSingleton<TelegramBotClient>(new TelegramBotClient("6464331195:AAEn8HfRPz7kl25Rnab-ZBK_0JnKWu9Ma28"));
services.AddTransient<TelegramService>();
services.AddTransient<TorrentService>();
services.AddSingleton<ClientEngine>();
services.AddCors();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

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
        //���� � �����, ��� ����� ��������� �����
        var uploadPath = $"{Directory.GetCurrentDirectory()}/uploads";
        // ������� ����� ��� �������� ������
        Directory.CreateDirectory(uploadPath);

        foreach (var file in files)
        {
            // ���� � ����� uploads
            string fullPath = $"{uploadPath}/{file.FileName}";

            // ��������� ���� � ����� uploads
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }
        await response.WriteAsync("����� ������� ���������");

        //string jobId = BackgroundJob.Enqueue(() => );
        //BackgroundJob.CountinueJobWith(jobId, () => );
    }
    else
    {
        await response.SendFileAsync("html/index.html");
    }
});*/

app.Run();
