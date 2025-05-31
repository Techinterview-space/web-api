namespace Web.Api.Features.Emails.ViewModels;

public record ReviewWasRejectedViewModel : ViewModelBase
{
    public const string ViewName = "/Views/EmailTemplates/ReviewWasRejected.cshtml";

    public const string Subject = "Ваш отзыв был отклонен";

    public ReviewWasRejectedViewModel(
        string companyName,
        string unsubscribeToken)
        : base(unsubscribeToken)
    {
        CompanyName = companyName;
    }

    public string CompanyName { get; }
}