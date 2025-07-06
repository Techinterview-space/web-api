namespace Infrastructure.Services.Html;

public class MarkdownToTelegramHtml
{
    public string Source { get; }

    public string Html { get; }

    public MarkdownToTelegramHtml(
        string source)
    {
        Source = source;
    }

    public override string ToString()
    {
        return Html;
    }
}