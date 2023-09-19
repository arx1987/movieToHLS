using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MovieToHLS.Services;
using MonoTorrent.Client;
using MovieToHLS;
using Telegram.Bot;
using MovieToHLS.Storage;
using System;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var config = builder.Configuration;

services.AddControllers().AddNewtonsoftJson();
services.AddDbContext<AppDBContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("postgres")));
services
    .AddTransient<AppDBContext>()
    .AddMemoryCache()
    .AddSingleton<TelegramBotClient>(new TelegramBotClient(config.GetSection("TelegramOptions:Token").Get<string>()))
    .AddTransient<TelegramService>()
    .AddTransient<TorrentService>()
    .AddSingleton<ClientEngine>()
    .AddTransient<Store>()
    .AddTransient<FileStorage>()
    .AddSingleton<IVideoConverter>(sp =>
    {
        var services = sp.GetRequiredService<IEnumerable<IHostedService>>();
        return services.OfType<BackgroundVideoConverter>().First();
    })
    .AddHostedService<TgWebhookRegistrator>()
    .AddHostedService<BackgroundVideoConverter>()
    .AddCors()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();


services.Configure<TelegramOptions>(config.GetSection(TelegramOptions.OptionName));

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateActor = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtExt.SecretKey,
        };
    });



var app = builder.Build();

app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.MapGet("/hello", async (HttpContext ctx) => $"{DateTime.Now}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();