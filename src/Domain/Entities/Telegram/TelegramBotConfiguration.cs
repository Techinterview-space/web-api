using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Telegram;

public class TelegramBotConfiguration : HasDatesBase, IHasIdBase<Guid>
{
    protected TelegramBotConfiguration()
    {
    }

    public TelegramBotConfiguration(
        TelegramBotType botType,
        string displayName,
        string token,
        bool isEnabled,
        string botUsername = null)
    {
        Id = Guid.NewGuid();
        BotType = botType;
        DisplayName = displayName;
        Token = token;
        IsEnabled = isEnabled;
        BotUsername = botUsername;
    }

    public Guid Id { get; protected set; }

    public TelegramBotType BotType { get; protected set; }

    [Required]
    [StringLength(200)]
    public string DisplayName { get; protected set; }

    [Required]
    [StringLength(500)]
    public string Token { get; protected set; }

    public bool IsEnabled { get; protected set; }

    [StringLength(200)]
    public string BotUsername { get; protected set; }

    public void Update(
        string displayName,
        bool isEnabled,
        string botUsername,
        string token = null)
    {
        DisplayName = displayName;
        IsEnabled = isEnabled;
        BotUsername = botUsername;

        if (!string.IsNullOrEmpty(token))
        {
            Token = token;
        }
    }
}
