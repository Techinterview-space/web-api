using System.Text.RegularExpressions;

namespace Infrastructure.Services.Html;

public class MarkdownToTelegramHtml
{
    public string Source { get; }

    public string Html { get; }

    public MarkdownToTelegramHtml(
        string source)
    {
        Source = source ?? string.Empty;
        Html = ConvertMarkdownToTelegramHtml(Source);
    }

    public override string ToString()
    {
        return Html;
    }

    private static string ConvertMarkdownToTelegramHtml(
        string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return string.Empty;
        }

        var result = markdown;

        // Convert headers to bold text
        result = ConvertHeadersToBold(result);

        // Convert markdown bold to <b> tags
        result = ConvertBoldText(result);

        // Convert markdown italic to <i> tags
        result = ConvertItalicText(result);

        // Convert code blocks to <pre> tags
        result = ConvertCodeBlocks(result);

        return result;
    }

    private static string ConvertHeadersToBold(
        string text)
    {
        // Match headers (# ## ### #### ##### ######) at start of line
        // Capture the header level and text
        const string headerPattern = @"^(#{1,6})\s+(.+)$";
        return Regex.Replace(text, headerPattern, "<b>$2</b>", RegexOptions.Multiline);
    }

    private static string ConvertBoldText(
        string text)
    {
        // Convert **text** to <b>text</b>
        // Use non-greedy matching to handle multiple bold sections on same line
        const string boldPattern = @"\*\*(.+?)\*\*";
        return Regex.Replace(text, boldPattern, "<b>$1</b>");
    }

    private static string ConvertItalicText(
        string text)
    {
        // Convert *text* to <i>text</i>
        // But avoid matching ** (bold) patterns
        // Use negative lookbehind and lookahead to avoid matching bold patterns
        const string italicPattern = @"(?<!\*)\*([^*]+?)\*(?!\*)";
        return Regex.Replace(text, italicPattern, "<i>$1</i>");
    }

    private static string ConvertCodeBlocks(
        string text)
    {
        // Convert ```language\ncode``` to <pre language="language">code</pre>
        // Also handle ```code``` (without language) to <pre>code</pre>

        // Pattern to match:
        // 1. ``` followed by optional language
        // 2. optional newline
        // 3. code content (anything until closing ```)
        // 4. closing ```
        const string codeBlockPattern = @"```(?:(\w+)\s*\n)?(.*?)```";

        return Regex.Replace(
            text,
            codeBlockPattern,
            match =>
            {
                var language = match.Groups[1].Value;
                var code = match.Groups[2].Value;

                // If language is specified, include it in the pre tag
                if (!string.IsNullOrWhiteSpace(language))
                {
                    return $"<pre language=\"{language}\">{code}</pre>";
                }

                return $"<pre>{code}</pre>";
            },
            RegexOptions.Singleline);
    }
}