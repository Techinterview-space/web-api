using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Aws.S3.Simple.Models;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Features.Files;

public record UploadFileServiceRequest : IUploadFileRequest
{
    private readonly IFormFile _file;

    public UploadFileServiceRequest(IFormFile file)
    {
        _file = file;
    }

    public long FileSize => _file.Length;

    public string FileName => _file.FileName;

    public string ContentType => _file.ContentType;

    public Stream OpenReadStream() => _file.OpenReadStream();

    public void CopyTo(Stream target) => _file.CopyTo(target);

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) =>
        _file.CopyToAsync(target, cancellationToken);
}

public record FileUploadRequest
{
    [Required]
    public IFormFile File { get; init; }
}
