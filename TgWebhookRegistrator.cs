using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace MovieToHLS;

public class TgWebhookRegistrator : IHostedService
{
    private readonly TelegramBotClient _tg;
    private readonly TelegramOptions _tgOptions;

    public TgWebhookRegistrator(TelegramBotClient tg, IOptions<TelegramOptions> tgOptions)
    {
        _tg = tg;
        _tgOptions = tgOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _tg.SetWebhookAsync($"{_tgOptions.HostUrl}/tg/webhook", cancellationToken: cancellationToken);
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}