using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Currencies.CreateCurrenciesCollection;
using Web.Api.Features.Currencies.DeleteCurrenciesCollection;
using Web.Api.Features.Currencies.Dtos;
using Web.Api.Features.Currencies.GetCurrenciesChartData;
using Web.Api.Features.Currencies.GetCurrenciesCollection;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Currencies;

[ApiController]
[Route("api/currencies")]
public class CurrenciesCollectionController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public CurrenciesCollectionController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("chart")]
    public Task<GetCurrenciesChartDataResponse> GetChartData(
        [FromQuery] GetCurrenciesChartDataQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<GetCurrenciesChartDataHandler, GetCurrenciesChartDataQueryParams, GetCurrenciesChartDataResponse>(
            queryParams ?? new GetCurrenciesChartDataQueryParams(),
            cancellationToken);
    }

    [HttpGet("")]
    [HasAnyRole(Role.Admin)]
    public Task<Pageable<CurrenciesCollectionDto>> GetAll(
        [FromQuery] GetCurrenciesCollectionQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<GetCurrenciesCollectionHandler, GetCurrenciesCollectionQueryParams, Pageable<CurrenciesCollectionDto>>(
            queryParams ?? new GetCurrenciesCollectionQueryParams(),
            cancellationToken);
    }

    [HttpPost("")]
    [HasAnyRole(Role.Admin)]
    public Task<CurrenciesCollectionDto> Create(
        [FromBody] CreateCurrenciesCollectionRequest request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<CreateCurrenciesCollectionHandler, CreateCurrenciesCollectionRequest, CurrenciesCollectionDto>(
            request,
            cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [HasAnyRole(Role.Admin)]
    public Task<bool> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<DeleteCurrenciesCollectionHandler, DeleteCurrenciesCollectionRequest, bool>(
            new DeleteCurrenciesCollectionRequest { Id = id },
            cancellationToken);
    }
}
