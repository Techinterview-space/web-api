using MediatR;

namespace Web.Api.Features.Companies.GetCompany;

public record GetCompanyQuery : IRequest<GetCompanyResponse>
{
    public GetCompanyQuery(
        string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; }
}