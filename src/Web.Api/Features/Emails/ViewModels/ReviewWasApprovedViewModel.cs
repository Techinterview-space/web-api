namespace Web.Api.Features.Emails.ViewModels;

public record ReviewWasApprovedViewModel : ViewModelBase
{
    public const string ViewName = "/Views/EmailTemplates/ReviewWasApproved.cshtml";

    public const string Subject = "Ваш отзыв одобрен";

    public ReviewWasApprovedViewModel(
        string companyName,
        string unsubscribeToken)
        : base(unsubscribeToken)
    {
        CompanyName = companyName;
    }

    public string CompanyName { get; }
}