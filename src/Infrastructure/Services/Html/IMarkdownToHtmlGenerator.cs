namespace Infrastructure.Services.Html;

public interface IMarkdownToHtmlGenerator
{
    string FromMarkdown(
        string source);
}