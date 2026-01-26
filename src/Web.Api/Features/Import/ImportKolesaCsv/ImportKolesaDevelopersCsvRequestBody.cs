using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public record ImportKolesaDevelopersCsvRequestBody
{
    [Required]
    public IFormFile File { get; init; }
}