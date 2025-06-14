﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.CSV;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Salaries.Providers;

namespace Web.Api.Features.Salaries.ExportCsv;

public class ExportCsvHandler : Infrastructure.Services.Mediator.IRequestHandler<Nothing, SalariesCsvResponse>
{
    private const string NoValue = "";

    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;
    private readonly ISalaryLabelsProvider _labelsProvider;

    public ExportCsvHandler(
        IAuthorization auth,
        DatabaseContext context,
        ISalaryLabelsProvider labelsProvider)
    {
        _auth = auth;
        _context = context;
        _labelsProvider = labelsProvider;
    }

    public async Task<SalariesCsvResponse> Handle(
        Nothing request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.GetCurrentUserOrNullAsync(cancellationToken);
        if (currentUser is null)
        {
            throw new NoPermissionsException("No user found");
        }

        var userCsvDownload = await _context.UserCsvDownloads
            .Where(x => x.UserId == currentUser.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCsvDownload is not null && !userCsvDownload.AllowsDownload(currentUser))
        {
            throw new NoPermissionsException(
                $"You have downloaded CSV file already withing last {UserCsvDownload.CountOfHoursToAllowDownload} hours.");
        }

        var salaries = await new SalariesForChartQuery(
            _context,
            null,
            null,
            null,
            null,
            DateTimeOffset.Now.AddMonths(-12),
            DateTimeOffset.Now,
            null,
            null,
            null)
            .ToQueryable()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var labels = await _labelsProvider.GetAsync(cancellationToken);
        var sb = new SalariesCsvStringBuilder(labels);
        sb.AppendHeader();

        foreach (var salary in salaries)
        {
            sb.AppendSalary(salary);
        }

        if (userCsvDownload is not null)
        {
            _context.UserCsvDownloads.Remove(userCsvDownload);
        }

        _context.UserCsvDownloads.Add(new UserCsvDownload(currentUser));
        await _context.SaveChangesAsync(cancellationToken);

        return new SalariesCsvResponse(sb.ToString());
    }
}