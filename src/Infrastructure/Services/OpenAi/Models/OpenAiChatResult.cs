namespace Infrastructure.Services.OpenAi.Models;

public record OpenAiChatResult
{
    public static OpenAiChatResult Success(List<Choice> choices)
    {
        return new OpenAiChatResult(true, choices);
    }

    public static OpenAiChatResult Failure()
    {
        return new OpenAiChatResult(false, new List<Choice>());
    }

    private OpenAiChatResult(
        bool isSuccess,
        List<Choice> choices)
    {
        IsSuccess = isSuccess;
        Choices = choices;
    }

    public bool IsSuccess { get; }

    public List<Choice> Choices { get; }
}