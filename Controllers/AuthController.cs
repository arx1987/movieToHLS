using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MovieToHLS.Services;
using Telegram.Bot;

namespace MovieToHLS.Controllers;

[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly Store _store;
    private readonly TelegramBotClient _telegram;
    private readonly IMemoryCache _memoryCache;

    public AuthController(Store store, TelegramBotClient telegram, IMemoryCache memoryCache)
    {
        _store = store;
        _telegram = telegram;
        _memoryCache = memoryCache;
    }

    [HttpPost("by-video-token/{token-id}")]
    public async Task Auth([FromRoute(Name = "token-id")] Guid tokenId)
    {
        var access = await _store.GetAccessOrNull(tokenId) ?? throw new ApplicationException("no access");
        var code = Random.Shared.Next(1000, 9999);

        _memoryCache.Set(AuthKey(tokenId), new TokenCode(tokenId, code, 0));

        await _telegram.SendTextMessageAsync(access.User.ChatId, $"Ваш код для авторизации: {code}");
    }

    [HttpPost("verify-code/{token-id:guid}/{code:int}")]
    public async Task<string> VerifyCode(
        [FromRoute(Name = "token-id")] Guid tokenId,
        int code)
    {
        var tokenCode = _memoryCache.Get<TokenCode>(AuthKey(tokenId))
                        ?? throw new ApplicationException("invalid code");

        if (tokenCode.Attempts >= 2)
            throw new ApplicationException("invalid code");

        if (tokenCode.Code != code)
        {
            _memoryCache.Set(AuthKey(tokenId), tokenCode with { Attempts = tokenCode.Attempts + 1 });
            throw new ApplicationException("invalid code");
        }

        var access = await _store.GetAccessOrNull(tokenId) ?? throw new ApplicationException("no access");

        return access.User.Id.CreateToken();//CreateToken() - it creates JWT Token
    }

    private string AuthKey(Guid tokenId) => $"auth-{tokenId.ToString()}";
}

public record TokenCode(Guid TokenId, int Code, int Attempts);