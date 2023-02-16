using System.Collections.Generic;
using Domain.Services.Global;

namespace TestUtils.Fakes;

#pragma warning disable SA1009
public record GlobalFake() : Global(new InMemoryConfig(new Dictionary<string, string>
{
    { "Global:Frontend", "https://techinterview.space" },
    { "Global:Backend", "https://api.techinterview.space" },
    { "Global:AppVersion", "22.4.1" },
    { "Global:AppName", "Tech.Interview" },
    { "Global:NoReplyEmail", "noreply@techinterview.space" },
    { "Global:AddDevEmailsToHiddenCc", "true" },
    { "Global:DeveloperEmails", "m.gorbatyuk@petrel.ai" },
    { "Global:EnableEmailPublishing", "true" },
}).Value());
#pragma warning restore SA1009