﻿using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Infrastructure.Services.Telegram;
using Infrastructure.Services.Telegram.Notifications;

namespace TestUtils.Services;

public class TelegramAdminNotificationServiceFake : ITelegramAdminNotificationService
{
    public Task NotifyAboutNewCompanyReviewAsync(
        CompanyReview review,
        Company company,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}