﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Enums;
using Domain.Files;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TechInterviewer.Controllers.Admin.Dtos;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers;

[ApiController]
[Route("api/admin-tools")]
[HasAnyRole(Role.Admin)]
public class AdminToolsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IPdf _pdf;
    private readonly IConfiguration _configuration;

    public AdminToolsController(
        DatabaseContext context,
        IPdf pdf,
        IConfiguration configuration)
    {
        _context = context;
        _pdf = pdf;
        _configuration = configuration;
    }

    [HttpPost("generate-from-html")]
    public async Task<IActionResult> GenerateFromHtmlAsync([FromBody] GenerateHtmlRequest request)
    {
        request
            .ThrowIfNull(nameof(request))
            .ThrowIfInvalid();

        var pdf = await _pdf.RenderAsync(request.Content, "demo.pdf", "application/pdf");
        return File(pdf.Data, contentType: pdf.ContentType, fileDownloadName: pdf.Filename);
    }

    [HttpGet("configs")]
    [HttpGet]
    public IActionResult GetConfigs()
    {
        var configs = GetSectionValues(_configuration.GetChildren());
        return Ok(configs);
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