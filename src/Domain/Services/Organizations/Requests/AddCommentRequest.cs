using System.ComponentModel.DataAnnotations;

namespace Domain.Services.Organizations.Requests;

public record AddCommentRequest
{
    [Required]
    [StringLength(CandidateCardComment.CommentLength)]
    public string Comment { get; init; }
}