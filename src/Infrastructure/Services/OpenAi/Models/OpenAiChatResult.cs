namespace Infrastructure.Services.OpenAi.Models;

public record OpenAiChatResult
{
    public static OpenAiChatResult Success(
        List<Choice> choices,
        string model)
    {
        return new OpenAiChatResult(
            choices.Count > 0,
            choices,
            model);
    }

    public static OpenAiChatResult Failure(
        string model)
    {
        return new OpenAiChatResult(
            false,
            new List<Choice>(),
            model);
    }

    private OpenAiChatResult(
        bool isSuccess,
        List<Choice> choices,
        string model)
    {
        IsSuccess = isSuccess;
        Choices = choices;
        Model = model;
    }

    public bool IsSuccess { get; }

    public List<Choice> Choices { get; }

    public string Model { get; }

    public string GetResponseTextOrNull()
    {
        return Choices.Count > 0
            ? Choices[0].Message.Content
            : null;
    }
}