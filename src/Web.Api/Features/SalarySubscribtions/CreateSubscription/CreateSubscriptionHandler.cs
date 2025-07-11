﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.StatData.Salary;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.SalarySubscribtions.GetSalarySubscriptions;

namespace Web.Api.Features.SalarySubscribtions.CreateSubscription;

public class CreateSubscriptionHandler
    : Infrastructure.Services.Mediator.IRequestHandler<CreateSalarySubscriptionCommand, SalarySubscriptionDto>
{
    private readonly DatabaseContext _context;

    public CreateSubscriptionHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<SalarySubscriptionDto> Handle(
        CreateSalarySubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (request.TelegramChatId == 0)
        {
            throw new BadRequestException("Telegram chat ID is required.");
        }

        var existingSubscription = await _context.SalariesSubscriptions
            .FirstOrDefaultAsync(
                x => x.TelegramChatId == request.TelegramChatId,
                cancellationToken);

        if (existingSubscription != null)
        {
            throw new BadRequestException("Subscription already exists.");
        }

        var professions = new List<long>();
        if (request.ProfessionIds is { Count: > 0 })
        {
            professions = await _context.Professions
                .Where(x => request.ProfessionIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        var newSubscription = _context.Add(
            new StatDataChangeSubscription(
                request.Name,
                request.TelegramChatId,
                professions,
                request.PreventNotificationIfNoDifference,
                request.Regularity,
                request.UseAiAnalysis));

        await _context.SaveChangesAsync(cancellationToken);
        return new SalarySubscriptionDto(newSubscription.Entity);
    }
}