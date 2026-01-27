namespace Web.Api.Features.Emails.ViewModels;

public record SimpleEmailViewModel
{
    public SimpleEmailViewModel(
        string body,
        string bodyTitle)
    {
        Body = body;
        BodyTitle = bodyTitle;
    }

    public string Body { get; }

    public string BodyTitle { get; }
}