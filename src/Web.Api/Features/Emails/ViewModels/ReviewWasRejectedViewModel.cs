namespace Web.Api.Features.Emails.ViewModels;

public record ReviewWasRejectedViewModel : ViewModelBase
{
    public const string ViewName = "/Views/EmailTemplates/ReviewWasRejected.cshtml";

    public const string Subject = "Ваш отзыв был отклонен";

    public ReviewWasRejectedViewModel(
        string companyName,
        string unsubscribeToken,
        string comment = null)
        : base(unsubscribeToken)
    {
        CompanyName = companyName;
        Comment = comment;
    }

    public string CompanyName { get; }

    public string Comment { get; }

    public bool HasComment => !string.IsNullOrWhiteSpace(Comment);
}