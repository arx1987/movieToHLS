namespace MovieToHLS;

public class TelegramOptions
{
    public const string OptionName = nameof(TelegramOptions);

    public string Token { get; set; }
    public string HostUrl { get; set; }
}