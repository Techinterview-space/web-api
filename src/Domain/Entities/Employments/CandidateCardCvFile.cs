using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Employments;

public record CandidateCardCvFile
{
    public CandidateCardCvFile()
    {
    }

    public CandidateCardCvFile(
        string fileName,
        string storageFileName)
    {
        Id = Guid.NewGuid();
        FileName = fileName;
        StorageFileName = storageFileName;
        CreatedAt = DateTimeOffset.Now;
    }

    public Guid Id { get; init; }

    [Required]
    public string FileName { get; init; }

    [Required]
    public string StorageFileName { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}