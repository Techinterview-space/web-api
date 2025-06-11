using System;
using MediatR;

namespace Web.Api.Features.Companies.UpdateCompany;

public record UpdateCompanyCommand : IRequest<Unit>
{
    public UpdateCompanyCommand(
        Guid companyId,
        EditCompanyBodyRequest body)
    {
        CompanyId = companyId;
        Body = body;
    }

    public Guid CompanyId { get; }

    public EditCompanyBodyRequest Body { get; }
}