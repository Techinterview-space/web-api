namespace Infrastructure.Services.Html;

public class MarkdownToHtmlGenerator : IMarkdownToHtmlGenerator
{
    public string FromMarkdown(
        string source)
    {
        return new MarkdownToHtml(source).ToString();
    }
}