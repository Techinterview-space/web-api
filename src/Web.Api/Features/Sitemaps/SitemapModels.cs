using System.Collections.Generic;
using System.Xml.Serialization;

namespace Web.Api.Features.Sitemaps;

[XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
public record SitemapUrlSet
{
    [XmlElement("url")]
    public List<SitemapUrl> Urls { get; set; } = new ();
}

public record SitemapUrl
{
    [XmlElement("loc")]
    public string Loc { get; set; }

    [XmlElement("lastmod")]
    public string LastMod { get; set; }

    [XmlElement("changefreq")]
    public string ChangeFreq { get; set; }

    [XmlElement("priority")]
    public string Priority { get; set; }
}
