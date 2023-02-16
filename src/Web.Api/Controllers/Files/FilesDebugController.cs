using System;
using System.Threading.Tasks;
using AspNetCore.Aws.S3.Simple.Models;
using FileService.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace TechInterviewer.Controllers.Files;

[ApiController]
[Route("api/debug/file/")]
public class FilesDebugController : ControllerBase
{
    private readonly IPublicStorage _storage;

    public FilesDebugController(
        IPublicStorage storage,
        IHostEnvironment environment)
    {
        if (environment.IsProduction())
        {
            throw new InvalidOperationException("This controller is not allowed in production.");
        }

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