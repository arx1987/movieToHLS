namespace MovieToHLS.Entities;

public class User
{
    public Guid Id { get; }
    public string ChatId { get; private set; }

    public User(Guid id, string chatId)
    {
        Id = id;
        ChatId = chatId;
    }

    public void SetChatId(string chatId)
    {
        ChatId = chatId;
    }
}