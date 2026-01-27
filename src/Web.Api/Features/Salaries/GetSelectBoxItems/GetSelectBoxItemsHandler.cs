using System.Threading;
using System.Threading.Tasks;
using Web.Api.Features.Salaries.Models;
using Web.Api.Features.Salaries.Providers;

namespace Web.Api.Features.Salaries.GetSelectBoxItems;

public class GetSelectBoxItemsHandler : Infrastructure.Services.Mediator.IRequestHandler<GetSelectBoxItemsQuery, SelectBoxItemsResponse>
{
    private readonly ISalaryLabelsProvider _provider;

    public GetSelectBoxItemsHandler(
        ISalaryLabelsProvider provider)
    {
        _provider = provider;
    }

    public Task<SelectBoxItemsResponse> Handle(
        GetSelectBoxItemsQuery request,
        CancellationToken cancellationToken)
    {
        return _provider.GetAsync(cancellationToken);
    }
}