using System;

namespace Web.Api.Features.Companies.UpdateCompany;

public record UpdateCompanyCommand
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