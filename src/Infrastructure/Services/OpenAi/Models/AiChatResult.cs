namespace Infrastructure.Services.OpenAi.Models;

public record AiChatResult
{
    public static AiChatResult Success(
        List<AiChoice> choices,
        string model)
    {
        return new AiChatResult(
            choices.Count > 0,
            choices,
            model);
    }

    public static AiChatResult Failure(
        string model)
    {
        return new AiChatResult(
            false,
            new List<AiChoice>(),
            model);
    }

    private AiChatResult(
        bool isSuccess,
        List<AiChoice> choices,
        string model)
    {
        IsSuccess = isSuccess;
        Choices = choices;
        Model = model;
    }

    public bool IsSuccess { get; }

    public List<AiChoice> Choices { get; }

    public string Model { get; }

    public string GetResponseTextOrNull()
    {
        return Choices.Count > 0
            ? Choices[0].Message.Content
            : null;
    }
}