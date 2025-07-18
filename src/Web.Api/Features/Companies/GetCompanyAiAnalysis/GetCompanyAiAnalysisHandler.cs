﻿using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Validation.Exceptions;
using Infrastructure.Ai;
using Infrastructure.Database;
using Infrastructure.Extensions;
using Infrastructure.Services.AiServices;
using Infrastructure.Services.Correlation;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.Companies.GetCompanyAiAnalysis;

public class GetCompanyAiAnalysisHandler
    : IRequestHandler<string, AiChatResult>
{
    private readonly IArtificialIntellectService _openAiService;
    private readonly ILogger<GetCompanyAiAnalysisHandler> _logger;
    private readonly DatabaseContext _context;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public GetCompanyAiAnalysisHandler(
        IArtificialIntellectService openAiService,
        ILogger<GetCompanyAiAnalysisHandler> logger,
        DatabaseContext context,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _openAiService = openAiService;
        _logger = logger;
        _context = context;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public async Task<AiChatResult> Handle(
        string companyId,
        CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .Include(x => x.RatingHistory)
            .Include(x => x.Reviews)
            .Include(x => x.OpenAiAnalysisRecords)
            .GetCompanyByIdentifierOrNullAsync(
                companyId,
                cancellationToken)
            ?? throw new NotFoundException("Company not found.");

        var aiResult = await _openAiService
            .AnalyzeCompanyAsync(
                company,
                _correlationIdAccessor.GetValue(),
                cancellationToken);

        if (aiResult.IsSuccess)
        {
            company.ClearAiAnalysisRecords();
            var aiRecord = _context.Add(
                new CompanyOpenAiAnalysis(
                    company,
                    aiResult.GetResponseTextOrNull(),
                    aiResult.Model)).Entity;

            await _context.SaveChangesAsync(cancellationToken);
        }

        return aiResult;
    }
}