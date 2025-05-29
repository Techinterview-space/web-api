namespace Web.Api.Features.Emails;

public record EmailPreviewResponse
{
    public EmailPreviewResponse(
        string html)
    {
        Html = html;
    }

    public string Html { get; }
}