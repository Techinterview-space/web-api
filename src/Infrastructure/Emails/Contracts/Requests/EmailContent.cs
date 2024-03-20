namespace Infrastructure.Emails.Contracts.Requests;

public record EmailContent
{
    // For asp features
    public EmailContent()
    {
    }

    public EmailContent(
        string from,
        string subject,
        string htmlBody,
        ICollection<string> recipients,
        ICollection<string> cc = null,
        ICollection<string> hiddenCc = null)
    {
        From = from;
        Subject = subject;
        HtmlBody = htmlBody;
        Recipients = recipients;
        Cc = cc ?? Array.Empty<string>();
        HiddenCc = hiddenCc ?? Array.Empty<string>();
    }

    public ICollection<string> Recipients { get; set; }

    public ICollection<string> Cc { get; set; }

    public ICollection<string> HiddenCc { get; set; }

    public string From { get; set; }

    public string Subject { get; set; }

    public string HtmlBody { get; set; }
}