using System.Threading;
using System.Threading.Tasks;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Providers;

public interface ISalaryLabelsProvider
{
    Task ResetCacheAsync(
        CancellationToken cancellationToken);

    Task<SelectBoxItemsResponse> GetAsync(
        CancellationToken cancellationToken);
}