using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TechInterviewer.Features.Salaries.Models;
using TechInterviewer.Features.Salaries.Providers;

namespace TechInterviewer.Features.Salaries.GetSelectBoxItems;

public class GetSelectBoxItemsHandler : IRequestHandler<GetSelectBoxItemsQuery, SelectBoxItemsResponse>
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