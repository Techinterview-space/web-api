using System.Collections.Generic;
using System.Xml.Serialization;

namespace Web.Api.Features.CompanyReviews.GetRecentReviews;

[XmlRoot("rss")]
public record RssChannel
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "2.0";

    [XmlElement("channel")]
    public Channel Channel { get; set; }
}

public record Channel
{
    [XmlElement("title")]
    public string Title { get; set; }

    [XmlElement("link")]
    public string Link { get; set; }

    [XmlElement("description")]
    public string Description { get; set; }

    [XmlElement("language")]
    public string Language { get; set; } = "en-us";

    [XmlElement("lastBuildDate")]
    public string LastBuildDate { get; set; }

    [XmlElement("generator")]
    public string Generator { get; set; } = "Tech Interview Space API";

    [XmlElement("webMaster")]
    public string WebMaster { get; set; } = "admin@techinterview.space";

    [XmlElement("managingEditor")]
    public string ManagingEditor { get; set; } = "admin@techinterview.space";

    [XmlElement("ttl")]
    public int TimeToLive { get; set; } = 60; // 1 hour

    [XmlElement("item")]
    public List<RssItem> Items { get; set; } = new List<RssItem>();
}

public record RssItem
{
    [XmlElement("title")]
    public string Title { get; set; }

    [XmlElement("link")]
    public string Link { get; set; }

    [XmlElement("description")]
    public string Description { get; set; }

    [XmlElement("pubDate")]
    public string PubDate { get; set; }

    [XmlElement("guid")]
    public string Guid { get; set; }

    [XmlElement("category")]
    public string Category { get; set; }
}