using System.ComponentModel.DataAnnotations;

namespace Web.Api.Features.Labels.Models;

public record LabelEntityEditRequest
{
    public long? Id { get; init; }

    [Required]
    [StringLength(50)]
    public string Title { get; init; }

    [Required]
    [StringLength(7)]
    public string HexColor { get; init; }
}