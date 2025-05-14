using System;
using MediatR;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.GetCompany;

public record GetCompanyQuery : IRequest<GetCompanyResponse>
{
    public GetCompanyQuery(
        string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; }

    public Guid? GetIdentifierAsGuid()
    {
        if (Guid.TryParse(Identifier, out var guid))
        {
            return guid;
        }

        return null;
    }
}