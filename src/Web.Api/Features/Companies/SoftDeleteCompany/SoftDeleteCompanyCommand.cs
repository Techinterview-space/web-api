using System;
using MediatR;

namespace Web.Api.Features.Companies.SoftDeleteCompany;

public record SoftDeleteCompanyCommand : IRequest<Unit>
{
    public SoftDeleteCompanyCommand(
        Guid companyId)
    {
        CompanyId = companyId;
    }

    public Guid CompanyId { get; }
}