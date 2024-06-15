using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Web.Api.Services.Swagger;

// https://stackoverflow.com/a/69089035
public class DescribeEnumMembers : ISchemaFilter
{
    public const string Prefix = "<p>Possible values:</p>";
    public const string Format = "<b>{0} - {1}</b>: {2}";

    private readonly XDocument _xmlComments;

    /// <summary>
    /// Initialize schema filter.
    /// </summary>
    /// <param name="xmlComments">Document containing XML docs for enum members.</param>
    public DescribeEnumMembers(XDocument xmlComments)
    {
        _xmlComments = xmlComments;
    }

    /// <summary>
    /// Apply this schema filter.
    /// </summary>
    /// <param name="schema">Target schema object.</param>
    /// <param name="context">Schema filter context.</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        // Only process enums and...
        if (!type.IsEnum)
        {
            return;
        }

        var sb = new StringBuilder(schema.Description);

        if (!string.IsNullOrEmpty(Prefix))
        {
            sb.AppendLine(Prefix);
        }

        sb.AppendLine("<ul>");

        foreach (var name in Enum.GetValues(type))
        {
            // Allows for large enums
            var value = Convert.ToInt64(name);
            var fullName = $"F:{type.FullName}.{name}";

            var description = _xmlComments.XPathEvaluate(
                $"normalize-space(//member[@name = '{fullName}']/summary/text())") as string;

            sb.AppendLine(string.Format("<li>" + Format + "</li>", value, name, description));
        }

        sb.AppendLine("</ul>");

        schema.Description = sb.ToString();
    }
}