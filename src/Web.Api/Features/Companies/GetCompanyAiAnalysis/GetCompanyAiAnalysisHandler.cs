using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Extensions;
using Infrastructure.Services.Correlation;
using Infrastructure.Services.Mediator;
using Infrastructure.Services.OpenAi;
using Infrastructure.Services.OpenAi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.Companies.GetCompanyAiAnalysis;

public class GetCompanyAiAnalysisHandler
    : IRequestHandler<string, OpenAiChatResult>
{
    private readonly IOpenAiService _openAiService;
    private readonly ILogger<GetCompanyAiAnalysisHandler> _logger;
    private readonly DatabaseContext _context;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public GetCompanyAiAnalysisHandler(
        IOpenAiService openAiService,
        ILogger<GetCompanyAiAnalysisHandler> logger,
        DatabaseContext context,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        _openAiService = openAiService;
        _logger = logger;
        _context = context;
        _correlationIdAccessor = correlationIdAccessor;
    }

    public async Task<OpenAiChatResult> Handle(
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

        var response = await _openAiService
            .AnalyzeCompanyAsync(
                company,
                _correlationIdAccessor.GetValue(),
                cancellationToken);

        if (response.IsSuccess)
        {
            company.ClearAiAnalysisRecords();
            var aiRecord = _context.Add(
                new CompanyOpenAiAnalysis(
                    company,
                    response.GetResponseTextOrNull(),
                    response.Model)).Entity;

            await _context.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}