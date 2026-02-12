using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.PublicSurveys.ClosePublicSurvey;
using Web.Api.Features.PublicSurveys.CreatePublicSurvey;
using Web.Api.Features.PublicSurveys.DeletePublicSurvey;
using Web.Api.Features.PublicSurveys.Dtos;
using Web.Api.Features.PublicSurveys.GetMyPublicSurveys;
using Web.Api.Features.PublicSurveys.GetPublicSurveyBySlug;
using Web.Api.Features.PublicSurveys.GetPublicSurveyResults;
using Web.Api.Features.PublicSurveys.GetPublicSurveys;
using Web.Api.Features.PublicSurveys.PublishPublicSurvey;
using Web.Api.Features.PublicSurveys.ReopenPublicSurvey;
using Web.Api.Features.PublicSurveys.RestorePublicSurvey;
using Web.Api.Features.PublicSurveys.SubmitPublicSurveyResponse;
using Web.Api.Features.PublicSurveys.UpdatePublicSurvey;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.PublicSurveys;

[ApiController]
[Route("api/public-surveys")]

public class PublicSurveysController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public PublicSurveysController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpPost]
    [HasAnyRole]
    public async Task<IActionResult> Create(
        [FromBody] CreatePublicSurveyRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        return Ok(
            await _serviceProvider.HandleBy<CreatePublicSurveyHandler, CreatePublicSurveyRequest, PublicSurveyDto>(
                request, cancellationToken));
    }

    [HttpGet("my-surveys")]
    [HasAnyRole]
    public async Task<IActionResult> GetMySurveys(
        [FromQuery] GetMyPublicSurveysQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetMyPublicSurveysHandler, GetMyPublicSurveysQuery, List<MySurveyListItemDto>>(
                query, cancellationToken));
    }

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetPublicSurveysQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetPublicSurveysHandler, GetPublicSurveysQuery, Pageable<MySurveyListItemDto>>(
                query, cancellationToken));
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetPublicSurveyBySlugHandler, string, PublicSurveyDto>(
                slug, cancellationToken));
    }

    [HttpPatch("{id:guid}")]
    [HasAnyRole]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdatePublicSurveyRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        return Ok(
            await _serviceProvider.HandleBy<UpdatePublicSurveyHandler, UpdatePublicSurveyCommand, PublicSurveyDto>(
                new UpdatePublicSurveyCommand(id, request), cancellationToken));
    }

    [HttpPost("{id:guid}/publish")]
    [HasAnyRole]
    public async Task<IActionResult> Publish(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<PublishPublicSurveyHandler, Guid, PublicSurveyDto>(
                id, cancellationToken));
    }

    [HttpPost("{id:guid}/close")]
    [HasAnyRole]
    public async Task<IActionResult> Close(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<ClosePublicSurveyHandler, Guid, PublicSurveyDto>(
                id, cancellationToken));
    }

    [HttpPost("{id:guid}/reopen")]
    [HasAnyRole]
    public async Task<IActionResult> Reopen(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<ReopenPublicSurveyHandler, Guid, PublicSurveyDto>(
                id, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasAnyRole]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _serviceProvider.HandleBy<DeletePublicSurveyHandler, Guid, Nothing>(
            id, cancellationToken);

        return Ok();
    }

    [HttpPost("{id:guid}/restore")]
    [HasAnyRole]
    public async Task<IActionResult> Restore(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<RestorePublicSurveyHandler, Guid, PublicSurveyDto>(
                id, cancellationToken));
    }

    [HttpPost("{slug}/responses")]
    [HasAnyRole]
    public async Task<IActionResult> SubmitResponse(
        [FromRoute] string slug,
        [FromBody] SubmitPublicSurveyResponseRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        await _serviceProvider.HandleBy<SubmitPublicSurveyResponseHandler, SubmitPublicSurveyResponseCommand, Nothing>(
            new SubmitPublicSurveyResponseCommand(slug, request), cancellationToken);

        return Ok();
    }

    [HttpGet("{slug}/results")]
    [HasAnyRole]
    public async Task<IActionResult> GetResults(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _serviceProvider.HandleBy<GetPublicSurveyResultsHandler, string, PublicSurveyResultsDto>(
                slug, cancellationToken));
    }
}
