using System;
using MediatR;
using Web.Api.Features.Companies.CreateCompany;

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