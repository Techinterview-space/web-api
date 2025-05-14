using MediatR;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.GetCompanyByAdmin;

public record GetCompanyByAdminQuery : IRequest<CompanyDto>
{
    public GetCompanyByAdminQuery(
        string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; }
}