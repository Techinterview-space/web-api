using System.Collections.Generic;
using MediatR;

namespace Web.Api.Features.Telegram.GetTelegramUserSettings;

public record GetTelegramUserSettingsQuery : IRequest<List<TelegramUserSettingsDto>>;