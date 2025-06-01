namespace Web.Api.Features.Emails.ViewModels;

public record SalaryUpdateReminderViewModel : ViewModelBase
{
    public const string ViewName = "/Views/EmailTemplates/SalaryUpdateReminder.cshtml";

    public const string Subject = "Techinterview - вы давно оставили анкету о зарплате";

    public SalaryUpdateReminderViewModel(
        string unsubscribeToken)
        : base(unsubscribeToken)
    {
    }
}