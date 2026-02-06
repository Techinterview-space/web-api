using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Features.Sitemaps;

[ApiController]
[Route("api")]
public class SitemapController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public SitemapController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("sitemap.xml")]
    [Produces("application/xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> GetSitemap(
        CancellationToken cancellationToken)
    {
        var sitemap = await _serviceProvider
            .HandleBy<GetSitemapHandler, Nothing, SitemapUrlSet>(
                Nothing.Value,
                cancellationToken);

        var xmlSerializer = new XmlSerializer(typeof(SitemapUrlSet));
        var stringBuilder = new StringBuilder();

        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, "http://www.sitemaps.org/schemas/sitemap/0.9");

        await using var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            OmitXmlDeclaration = false,
            Async = true,
        });

        xmlSerializer.Serialize(writer, sitemap, namespaces);

        return Content(
            stringBuilder.ToString(),
            "application/xml; charset=utf-8");
    }
}
