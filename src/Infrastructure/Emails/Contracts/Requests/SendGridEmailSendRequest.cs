﻿namespace Infrastructure.Emails.Contracts.Requests;

public record SendGridEmailSendRequest
{
    public SendGridEmailSendRequest(
        string subject,
        string recipient,
        string body,
        ICollection<string> cc = null,
        ICollection<string> hiddenCc = null)
        : this(
            subject,
            new List<string> { recipient },
            body,
            cc,
            hiddenCc)
    {
    }

    public SendGridEmailSendRequest(
        string subject,
        ICollection<string> recipients,
        string body,
        ICollection<string> cc = null,
        ICollection<string> hiddenCc = null)
    {
        Subject = subject;
        Recipients = recipients;
        Body = body;
        Cc = cc ?? new List<string>();
        HiddenCc = hiddenCc ?? new List<string>();
    }

    public ICollection<string> Recipients { get; }

    public ICollection<string> Cc { get; }

    public ICollection<string> HiddenCc { get; }

    public string Subject { get; }

    public string Body { get; }
}