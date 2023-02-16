using System;
using System.Collections.Generic;

namespace Domain.Services.Global;

public interface IGlobal
{
    string AppName { get; }

    string NoReplyEmail { get; }

    string FrontendBaseUrl { get; }

    string BackendBaseUrl { get; }

    string AppVersion { get; }

    IReadOnlyCollection<string> DeveloperEmails { get; }

    bool AddDevEmailsToHiddenCc { get; }

    bool EnableEmailPublishing { get; }

    string InterviewWebLink(Guid id);

    string OrganizationWebLink(Guid id);

    string ProfileWebLink();
}