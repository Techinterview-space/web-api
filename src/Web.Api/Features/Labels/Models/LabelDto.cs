using System.ComponentModel.DataAnnotations;
using Domain.Entities.Labels;
using Domain.ValueObjects;

namespace Web.Api.Features.Labels.Models;

public record LabelDto
{
    public LabelDto()
    {
    }

    // for test purposes
    public LabelDto(
        string title,
        HexColor color = null)
    {
        Title = title;
        HexColor = (color ?? Domain.ValueObjects.HexColor.Random()).ToString();
    }

    public LabelDto(
        EntityLabelBase label)
    {
        Id = label.Id;
        Title = label.Title;
        HexColor = label.HexColor.ToString();
        CreatedById = label.CreatedById;
    }

    public long? Id { get; init; }

    [Required]
    [StringLength(50)]
    public string Title { get; init; }

    [Required]
    [StringLength(7)]
    public string HexColor { get; init; }

    public long? CreatedById { get; init; }
}