using System.Threading;
using System.Threading.Tasks;
using TechInterviewer.Features.Salaries.Models;

namespace TechInterviewer.Features.Salaries.Providers;

public interface ISalaryLabelsProvider
{
    Task ResetCacheAsync(
        CancellationToken cancellationToken);

    Task<SelectBoxItemsResponse> GetAsync(
        CancellationToken cancellationToken);
}