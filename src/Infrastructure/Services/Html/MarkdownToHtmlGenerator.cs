﻿using Markdig;

namespace Infrastructure.Services.Html;

public class MarkdownToHtmlGenerator : IMarkdownToHtmlGenerator
{
    public string FromMarkdown(
        string source)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        return Markdown.ToHtml(source, pipeline);
    }
}