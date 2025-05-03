using Domain.Entities.Salaries;
using Domain.Entities.StatData;
using Domain.Extensions;
using Infrastructure.Database;
using Infrastructure.Services.Telegram.UserCommands;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Salaries;

public record SalarySubscriptionData
{
    private readonly StatDataChangeSubscription _subscription;
    private readonly DatabaseContext _context;
    private readonly SalariesForChartQuery _salariesForChartQuery;

    private bool _isInitianlized;

    public TelegramBotUserCommandParameters FilterData { get; }

    public List<StatDataChangeSubscriptionRecord> LastCacheItems { get; private set; }

    public StatDataChangeSubscriptionRecord LastCacheItemOrNull { get; private set; }

    public List<SalaryBaseData> Salaries { get; private set; }

    public int TotalSalaryCount { get; private set; }

    public SalarySubscriptionData(
        List<Profession> allProfessions,
        StatDataChangeSubscription subscription,
        DatabaseContext context,
        DateTimeOffset now)
    {
        _subscription = subscription;
        _context = context;
        _isInitianlized = false;

        FilterData = new TelegramBotUserCommandParameters(
            allProfessions
                .When(
                    subscription.ProfessionIds != null &&
                    subscription.ProfessionIds.Count > 0,
                    x => subscription.ProfessionIds.Contains(x.Id))
                .ToList());

        _salariesForChartQuery = new SalariesForChartQuery(
            _context,
            FilterData,
            now);
    }

    public async Task<SalarySubscriptionData> InitializeAsync(
        CancellationToken cancellationToken)
    {
        LastCacheItems = await _context.StatDataChangeSubscriptionRecords
            .AsNoTracking()
            .Where(x => x.SubscriptionId == _subscription.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .ToListAsync(cancellationToken);

        LastCacheItemOrNull = LastCacheItems.FirstOrDefault();

        TotalSalaryCount = await _salariesForChartQuery.CountAsync(cancellationToken);
        Salaries = await _salariesForChartQuery
            .ToQueryable(CompanyType.Local)
            .Where(x => x.Grade.HasValue)
            .Select(x => new SalaryBaseData
            {
                ProfessionId = x.ProfessionId,
                Grade = x.Grade.Value,
                Value = x.Value,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        _isInitianlized = true;
        return this;
    }

    public SalarySubscriptionData IsInitializedOrFail()
    {
        if (_isInitianlized)
        {
            return this;
        }

        throw new InvalidOperationException(
            $"SalarySubscriptionData is not initialized. Call {nameof(InitializeAsync)}() method first.");
    }

    public StatDataCacheItemSalaryData GetStatDataCacheItemSalaryData()
    {
        IsInitializedOrFail();
        return new StatDataCacheItemSalaryData(
            Salaries,
            TotalSalaryCount);
    }
}