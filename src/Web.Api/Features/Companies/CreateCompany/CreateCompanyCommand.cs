using System.Collections.Generic;
using Domain.Validation;
using MediatR;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.CreateCompany;

public record CreateCompanyCommand : EditCompanyBodyRequest, IRequest<CompanyDto>
{
    public CreateCompanyCommand(
        EditCompanyBodyRequest request)
    {
        request.ThrowIfInvalid();

        Name = request.Name;
        Description = request.Description;
        Links = request.Links ?? new List<string>();
        LogoUrl = request.LogoUrl;
    }
}