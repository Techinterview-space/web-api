using Markdig;

namespace Infrastructure.Services.Html;

public record MarkdownToHtml
{
    public string Source { get; }

    public string Html { get; }

    public MarkdownToHtml(
        string source)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        Html = Markdown.ToHtml(source, pipeline);
        Source = source;
    }

    public override string ToString()
    {
        return Html;
    }
}