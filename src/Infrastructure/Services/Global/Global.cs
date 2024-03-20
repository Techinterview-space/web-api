using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Global;

public record Global : IGlobal
{
    public string AppName { get; }

    public string NoReplyEmail { get; }

    public string FrontendBaseUrl { get; }

    public string BackendBaseUrl { get; }

    public string AppVersion { get; }

    public IReadOnlyCollection<string> DeveloperEmails { get; }

    public bool AddDevEmailsToHiddenCc { get; }

    public bool EnableEmailPublishing { get; }

    public string InterviewWebLink(Guid id) => $"{FrontendBaseUrl}/interviews/{id}";

    public string OrganizationWebLink(Guid id) => $"{FrontendBaseUrl}/organizations/{id}";

    public string ProfileWebLink() => $"{FrontendBaseUrl}/me";

    public Global(
        IConfiguration configuration)
    {
        AppName = configuration["Global:AppName"];
        NoReplyEmail = configuration["Global:NoReplyEmail"];
        FrontendBaseUrl = configuration["Global:Frontend"];
        BackendBaseUrl = configuration["Global:Backend"];
        AppVersion = configuration["Global:AppVersion"];
        DeveloperEmails = configuration["Global:DeveloperEmails"].Split(',');
        AddDevEmailsToHiddenCc = bool.TryParse(configuration["Global:AppName"], out var hiddenCc) && hiddenCc;
        EnableEmailPublishing = bool.TryParse(configuration["Global:EnableEmailPublishing"], out var emailPublish) && emailPublish;
    }
}