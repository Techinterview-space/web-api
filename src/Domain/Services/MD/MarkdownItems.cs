using System.Text;

namespace Domain.Services.MD;

public static class MarkdownItems
{
    public static string Line() => "---";

    public static string Link(string url, string text) => $"[{text}]({url})";

    public static string H1(string text) => $"# {text}";

    public static string H2(string text) => $"## {text}";

    public static string H3(string text) => $"### {text}";

    public static string H4(string text) => $"#### {text}";

    public static string H5(string text) => $"##### {text}";

    public static string H6(string text) => $"###### {text}";

    public static string List(params string[] lines)
    {
        var builder = new StringBuilder();
        foreach (var line in lines)
        {
            builder.AppendLine($"- {line}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public static string Code(string code) => $"```\n{code}\n```";

    public static string Italic(string source) => $"*{source}*";

    public static string Bold(string source) => $"**{source}**";
}