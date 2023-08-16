using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace MovieToHLS.Services;

public class TelegramService
{
    private readonly TelegramBotClient _telegram;

    public TelegramService(TelegramBotClient telegram)
    {
        _telegram = telegram;
    }
    public async Task Notify(string message) //InlineKeyboardMarkup keyboard)
    {
        await _telegram.SendTextMessageAsync("677661232", message); //, replyMarkup: keyboard);
    }
}