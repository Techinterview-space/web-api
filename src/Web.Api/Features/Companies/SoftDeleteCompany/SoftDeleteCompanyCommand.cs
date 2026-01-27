using System;

namespace Web.Api.Features.Companies.SoftDeleteCompany;

public record SoftDeleteCompanyCommand
{
    public SoftDeleteCompanyCommand(
        Guid companyId)
    {
        CompanyId = companyId;
    }

    public Guid CompanyId { get; }
}