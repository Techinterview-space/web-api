using System;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Employments;
using Domain.Entities.Users;
using MG.Utils.Entities;

namespace Domain.Services.Organizations;

public class CandidateCardComment : BaseModel
{
    public const int CommentLength = 10_000;

    protected CandidateCardComment()
    {
    }

    public CandidateCardComment(
        User author,
        CandidateCard candidateCard,
        string comment)
    {
        AuthorId = author.Id;
        Comment = comment?.Trim();
        CandidateCardId = candidateCard.Id;
    }

    [Required]
    [StringLength(CommentLength)]
    public string Comment { get; protected set; }

    public long AuthorId { get; protected set; }

    public virtual User Author { get; protected set; }

    public Guid CandidateCardId { get; protected set; }

    public virtual CandidateCard CandidateCard { get; protected set; }

    public bool IsAuthor(User user)
    {
        return AuthorId == user.Id;
    }
}