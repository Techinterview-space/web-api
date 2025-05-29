namespace Web.Api.Features.Emails.ViewModels;

public record ReviewWasApprovedViewModel
{
    public const string ViewName = "/Views/EmailTemplates/ReviewWasApproved.cshtml";

    public ReviewWasApprovedViewModel(
        string companyName)
    {
        CompanyName = companyName;
    }

    public string CompanyName { get; }
}