using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Database;
using Domain.Enums;
using MG.Utils.Abstract;
using MG.Utils.Export.Pdf;
using MG.Utils.Validation;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Controllers.Admin.Dtos;

namespace TechInterviewer.Controllers;

[ApiController]
[Route("api/admin-tools")]
[HasAnyRole(Role.Admin)]
public class AdminToolsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IPdf _pdf;

    public AdminToolsController(DatabaseContext context, IPdf pdf)
    {
        _context = context;
        _pdf = pdf;
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
}