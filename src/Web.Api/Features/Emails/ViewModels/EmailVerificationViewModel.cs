namespace Web.Api.Features.Emails.ViewModels;

public record EmailVerificationViewModel
{
    public const string ViewName = "/Views/EmailTemplates/EmailVerification.cshtml";

    public const string Subject = "Verify your email - TechInterview.space";

    public EmailVerificationViewModel(
        string firstName,
        string verificationUrl)
    {
        FirstName = firstName;
        VerificationUrl = verificationUrl;
    }

    public string FirstName { get; }

    public string VerificationUrl { get; }
}
