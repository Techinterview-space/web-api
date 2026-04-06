namespace Web.Api.Features.Emails.ViewModels;

public record PasswordResetViewModel : ViewModelBase
{
    public const string ViewName = "/Views/EmailTemplates/PasswordReset.cshtml";

    public const string Subject = "Reset your password - TechInterview.space";

    public PasswordResetViewModel(
        string firstName,
        string resetUrl,
        string unsubscribeToken = null)
        : base(unsubscribeToken)
    {
        FirstName = firstName;
        ResetUrl = resetUrl;
    }

    public string FirstName { get; }

    public string ResetUrl { get; }
}
