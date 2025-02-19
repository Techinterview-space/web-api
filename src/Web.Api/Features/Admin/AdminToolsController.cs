﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Images;
using Infrastructure.Services.Files;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Web.Api.Features.Admin.Models;
using Web.Api.Features.Admin.QR;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Admin;

[ApiController]
[Route("api/admin-tools")]
[HasAnyRole(Role.Admin)]
public class AdminToolsController : ControllerBase
{
    private readonly IPdf _pdf;
    private readonly IConfiguration _configuration;
    private readonly ICurrencyService _currencyService;

    public AdminToolsController(
        IPdf pdf,
        IConfiguration configuration,
        ICurrencyService currencyService)
    {
        _pdf = pdf;
        _configuration = configuration;
        _currencyService = currencyService;
    }

    [HttpGet("currencies")]
    public async Task<List<CurrencyContent>> GetCurrenciesAsync(
        CancellationToken cancellationToken)
    {
        return await _currencyService.GetAllCurrenciesAsync(cancellationToken);
    }

    [HttpPost("generate-from-html")]
    public IActionResult GenerateFromHtml(
        [FromBody] GenerateHtmlRequest request)
    {
        request
            .ThrowIfNull(nameof(request))
            .ThrowIfInvalid();

        var pdf = _pdf.Render(
            request.Content,
            "demo.pdf",
            "application/pdf");

        return File(pdf.Data, contentType: pdf.ContentType, fileDownloadName: pdf.Filename);
    }

    [HttpGet("configs")]
    public IActionResult GetConfigs()
    {
        var configs = GetSectionValues(_configuration.GetChildren());
        return Ok(configs);
    }

    [HttpPost("generate-qr")]
    public GenerateQRCodeResponse GenerateQrCode(
        [FromBody] GenerateQRCodeRequestBody requestBody)
    {
        var qr = new QRCodeImage(requestBody.Value);
        var qrImage = qr.AsBase64(requestBody.PixelSize);

        return new GenerateQRCodeResponse(qrImage);
    }

    private IDictionary<string, object> GetSectionValues(IEnumerable<IConfigurationSection> sections)
    {
        var result = new Dictionary<string, object>();

        foreach (var section in sections)
        {
            // Check if the current section has children (is a complex section)
            var children = section.GetChildren().ToList();
            if (children.Any())
            {
                // If the section has children, recursively get their values
                result.Add(section.Key, GetSectionValues(children));
            }
            else
            {
                // If the section does not have children, it's a key-value pair
                result.Add(section.Key, section.Value);
            }
        }

        return result;
    }
}