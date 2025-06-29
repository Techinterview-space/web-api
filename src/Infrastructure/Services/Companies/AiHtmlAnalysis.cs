using Domain.Entities.Companies;
using Infrastructure.Services.Html;

namespace Infrastructure.Services.Companies;

public record AiHtmlAnalysis
{
    public string Html { get; }

    public string Text { get; }

    public string Model { get; }

    public DateTimeOffset CreatedAt { get; }

    public AiHtmlAnalysis(
        Company company)
    {
        var latestAnalysis = company.OpenAiAnalysisRecords
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (latestAnalysis == null)
        {
            throw new InvalidOperationException("No OpenAI analysis records found for the company.");
        }

        Text = latestAnalysis.AnalysisText;
        Html = new MarkdownToHtml(latestAnalysis.AnalysisText).ToString();
        CreatedAt = latestAnalysis.CreatedAt;
        Model = latestAnalysis.Model;
    }
}