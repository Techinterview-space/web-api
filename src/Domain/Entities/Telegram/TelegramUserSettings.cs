using System;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Users;

namespace Domain.Entities.Telegram;

public class TelegramUserSettings : HasDatesBase, IHasIdBase<Guid>
{
    protected TelegramUserSettings()
    {
    }

    public TelegramUserSettings(
        string username,
        long chatId,
        User user,
        bool sendBotRegularStatsUpdates = false)
    {
        Username = username;
        ChatId = chatId;
        UserId = user.Id;
        User = user;
        SendBotRegularStatsUpdates = sendBotRegularStatsUpdates;
    }

    [Required]
    [StringLength(200)]
    public string Username { get; protected set; }

    public long ChatId { get; protected set; }

    public long UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public bool SendBotRegularStatsUpdates { get; protected set; }

    public Guid Id { get; protected set; }

    public void Update(
        bool sendBotRegularStatsUpdates)
    {
        SendBotRegularStatsUpdates = sendBotRegularStatsUpdates;
        UpdatedAt = DateTime.UtcNow;
    }
}