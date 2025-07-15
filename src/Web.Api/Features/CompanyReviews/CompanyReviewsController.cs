using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Domain.Entities.Companies;
using Domain.Enums;
using Domain.Validation;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Companies.Dtos;
using Web.Api.Features.CompanyReviews.AddCompanyReview;
using Web.Api.Features.CompanyReviews.ApproveReview;
using Web.Api.Features.CompanyReviews.DeleteCompanyReview;
using Web.Api.Features.CompanyReviews.GetRecentReviews;
using Web.Api.Features.CompanyReviews.MarkReviewOutdated;
using Web.Api.Features.CompanyReviews.SearchReviewsToBeApproved;
using Web.Api.Features.CompanyReviews.VoteForReview;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.CompanyReviews;

[ApiController]
[Route("api/companies")]
public class CompanyReviewsController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public CompanyReviewsController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("reviews/recent")]
    public async Task<IActionResult> SearchRecentReviews(
        [FromQuery] PageModel queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetRecentCompanyReviewsHandler, GetRecentCompanyReviewsQuery, Pageable<CompanyReviewDto>>(
                new GetRecentCompanyReviewsQuery(queryParams),
                cancellationToken));
    }

    [HttpGet("reviews/recent.rss")]
    [Produces("application/rss+xml")]
    public async Task<IActionResult> GetRecentReviewsRss(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var rssData = await _serviceProvider.HandleBy<GetRecentCompanyReviewsRssHandler, GetRecentCompanyReviewsRssQuery, RssChannel>(
            new GetRecentCompanyReviewsRssQuery(page, pageSize),
            cancellationToken);

        var xmlSerializer = new XmlSerializer(typeof(RssChannel));
        var stringBuilder = new StringBuilder();

        await using var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            OmitXmlDeclaration = false,
            Async = true
        });

        xmlSerializer.Serialize(writer, rssData);

        return Content(
            stringBuilder.ToString(),
            "application/rss+xml; charset=utf-8");
    }

    [HttpPost("{companyId:guid}/reviews")]
    [HasAnyRole]
    public async Task<IActionResult> AddCompanyReview(
        [FromRoute] Guid companyId,
        [FromBody] AddCompanyReviewBodyRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        await _serviceProvider.HandleBy<AddCompanyReviewHandler, AddCompanyReviewCommand, Nothing>(
            new AddCompanyReviewCommand(companyId, request),
            cancellationToken);

        return Ok();
    }

    [HttpGet("reviews/to-approve")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> SearchReviewsToBeApproved(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<SearchReviewsToBeApprovedHandler, Nothing, List<CompanyReviewDto>>(
                Nothing.Value,
                cancellationToken: cancellationToken));
    }

    [HttpPost("{companyId:guid}/reviews/{reviewId:guid}/approve")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> ApproveReview(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<ApproveReviewHandler, ApproveReviewCommand, Nothing>(
            new ApproveReviewCommand(companyId, reviewId),
            cancellationToken);

        return Ok();
    }

    [HttpPost("{companyId:guid}/reviews/{reviewId:guid}/outdate")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> MarkReviewOutdated(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<MarkReviewOutdatedHandler, MarkReviewOutdatedCommand, Nothing>(
            new MarkReviewOutdatedCommand(companyId, reviewId),
            cancellationToken);

        return Ok();
    }

    [HttpPost("{companyId:guid}/reviews/{reviewId:guid}/like")]
    [HasAnyRole]
    public async Task<IActionResult> LikeReview(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        var result = await _serviceProvider.HandleBy<VoteForReviewHandler, VoteForReviewCommand, VoteForReviewResponse>(
            new VoteForReviewCommand(
                companyId,
                reviewId,
                CompanyReviewVoteType.Like),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{companyId:guid}/reviews/{reviewId:guid}/dislike")]
    [HasAnyRole]
    public async Task<IActionResult> DislikeReview(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        var result = await _serviceProvider.HandleBy<VoteForReviewHandler, VoteForReviewCommand, VoteForReviewResponse>(
            new VoteForReviewCommand(
                companyId,
                reviewId,
                CompanyReviewVoteType.Dislike),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{companyId:guid}/reviews/{reviewId:guid}")]
    [HasAnyRole(Role.Admin)]
    public async Task<IActionResult> DeleteCompanyReview(
        [FromRoute] Guid companyId,
        [FromRoute] Guid reviewId,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeleteCompanyReviewHandler, DeleteCompanyReviewCommand, Nothing>(
            new DeleteCompanyReviewCommand(companyId, reviewId),
            cancellationToken);

        return Ok();
    }
}