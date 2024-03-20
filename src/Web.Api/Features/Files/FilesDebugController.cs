using System.Threading.Tasks;
using AspNetCore.Aws.S3.Simple.Models;
using Domain.Enums;
using Infrastructure.Services.Files;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Features.Files;

[ApiController]
[Route("api/debug/file/")]
[HasAnyRole(Role.Admin)]
public class FilesDebugController : ControllerBase
{
    private readonly IPublicStorage _storage;

    public FilesDebugController(
        IPublicStorage storage)
    {
        _storage = storage;
    }

    [HttpPost("upload")]
    public async Task<FileUploadResult> UploadAsync([FromForm] FileUploadRequest request)
    {
        return await _storage.UploadFileAsync(new UploadFileServiceRequest(request.File));
    }

    [HttpPost("download")]
    public async Task<FileContentResult> DownloadAsync([FromBody] FileDownloadRequest request)
    {
        var file = await _storage.DownloadFileAsync(request.Filename);
        return File(file.Content, file.ContentType, file.OriginalFileName);
    }
}