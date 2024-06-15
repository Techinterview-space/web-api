using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Currencies.Contracts;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs;

public class CurrenciesResetJob : InvocableJobBase<CurrenciesResetJob>
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesResetJob(
        ILogger<CurrenciesResetJob> logger,
        ICurrencyService currencyService)
        : base(logger)
    {
        _currencyService = currencyService;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        await _currencyService.ResetCacheAsync(cancellationToken);
    }
}