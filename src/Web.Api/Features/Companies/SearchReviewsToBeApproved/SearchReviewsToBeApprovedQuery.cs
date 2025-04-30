using System.Collections.Generic;
using MediatR;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.SearchReviewsToBeApproved;

public record SearchReviewsToBeApprovedQuery : IRequest<List<CompanyReviewDto>>;