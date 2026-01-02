using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Currencies.Contracts;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs;

public class RefetchServiceCurrenciesJob : InvocableJobBase<RefetchServiceCurrenciesJob>
{
    private readonly ICurrencyService _currencyService;

    public RefetchServiceCurrenciesJob(
        ILogger<RefetchServiceCurrenciesJob> logger,
        ICurrencyService currencyService)
        : base(logger)
    {
        _currencyService = currencyService;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        await _currencyService.RefetchServiceCurrenciesAsync(cancellationToken);
    }
}