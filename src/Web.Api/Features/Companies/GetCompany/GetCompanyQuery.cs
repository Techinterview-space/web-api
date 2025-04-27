using System;
using MediatR;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.GetCompany;

public record GetCompanyQuery : IRequest<CompanyDto>
{
    public GetCompanyQuery(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}