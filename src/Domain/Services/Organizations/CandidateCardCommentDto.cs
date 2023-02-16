using System;

namespace Domain.Services.Organizations;

public record CandidateCardCommentDto
{
    public CandidateCardCommentDto()
    {
    }

    public CandidateCardCommentDto(
        CandidateCardComment comment)
    {
        Id = comment.Id;
        Comment = comment.Comment;
        CreatedAt = comment.CreatedAt;
        AuthorId = comment.AuthorId;
        AuthorName = comment.Author?.Fullname;
        CandidateCardId = comment.CandidateCardId;
    }

    public long Id { get; }

    public string Comment { get; }

    public long AuthorId { get; }

    public string AuthorName { get; }

    public Guid CandidateCardId { get; }

    public DateTimeOffset CreatedAt { get; }
}