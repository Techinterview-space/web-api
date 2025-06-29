using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.OpenAI;

public class OpenAiPrompt : HasDatesBase
{
    public const string DefaultCompanyAnalyzePrompt =
        "You are a helpful career assistant. " +
        "Analyze the company's reviews and provide " +
        "a summary with advice what should user pay more attention on. " +
        "In the request there will be a company total rating, rating history and reviews presented in JSON format. Your reply should be in Russian language, markdown formatted.";

    public const string DefaultChatAnalyzePrompt =
        "You are a helpful assistant. Analyze the user's input and provide a response. " +
        "Your reply should be in question language, markdown formatted.";

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public OpenAiPromptType Id { get; protected set; }

    public string Prompt { get; protected set; }

    public OpenAiPrompt(
        OpenAiPromptType id,
        string prompt)
    {
        Id = id;
        Prompt = prompt;
    }

    // for migrations
    public OpenAiPrompt(
        OpenAiPromptType id,
        string prompt,
        DateTimeOffset createdAt)
    {
        Id = id;
        Prompt = prompt;
        CreatedAt = UpdatedAt = createdAt;
    }

    protected OpenAiPrompt()
    {
    }
}