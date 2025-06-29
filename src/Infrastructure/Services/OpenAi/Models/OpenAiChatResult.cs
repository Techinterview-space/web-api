namespace Infrastructure.Services.OpenAi.Models;

public record OpenAiChatResult
{
    public static OpenAiChatResult Success(List<Choice> choices)
    {
        return new OpenAiChatResult(choices.Count > 0, choices);
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

    public string GetResponseTextOrNull()
    {
        return Choices.Count > 0
            ? Choices[0].Message.Content
            : null;
    }
}