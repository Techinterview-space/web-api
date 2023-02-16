namespace Domain.Services.Html;

public interface ITechInterviewHtmlGenerator
{
    string FromMarkdown(
        string source);
}