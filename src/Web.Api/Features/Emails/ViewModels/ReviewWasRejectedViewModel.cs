namespace Web.Api.Features.Emails.ViewModels;

public record ReviewWasRejectedViewModel
{
    public const string ViewName = "/Views/EmailTemplates/ReviewWasRejected.cshtml";

    public ReviewWasRejectedViewModel(
        string companyName)
    {
        CompanyName = companyName;
    }

    public string CompanyName { get; }
}