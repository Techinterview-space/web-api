using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Domain.Enums;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.CompanyReviews.GetRecentReviews;
using Xunit;

namespace Web.Api.Tests.Features.CompanyReviews;

public class GetRecentCompanyReviewsRssHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnValidRssChannel_WhenReviewsExist()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();
        var handler = new GetRecentCompanyReviewsRssHandler(context);

        var company = new CompanyFake().Please(context);
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        // Approve the review to make it appear in RSS
        var review = new CompanyReviewFake(company, user)
            .SetApprovedAt(DateTime.UtcNow)
            .Please(context);

        var query = new GetRecentCompanyReviewsRssQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Channel);
        Assert.Equal("Tech Interview Space - Recent Company Reviews", result.Channel.Title);
        Assert.Equal("https://techinterview.space", result.Channel.Link);
        Assert.Equal("Latest company reviews from Tech Interview Space", result.Channel.Description);
        Assert.Single(result.Channel.Items);

        var item = result.Channel.Items[0];
        Assert.Contains(company.Name, item.Title);
        Assert.Contains(company.Slug, item.Link);
        Assert.NotEmpty(item.Description);
        Assert.NotEmpty(item.PubDate);
        Assert.Equal("Company Reviews", item.Category);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyItems_WhenNoApprovedReviewsExist()
    {
        // Arrange
        await using var context = new InMemoryDatabaseContext();
        var handler = new GetRecentCompanyReviewsRssHandler(context);

        var query = new GetRecentCompanyReviewsRssQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Channel);
        Assert.Empty(result.Channel.Items);
    }

    [Fact]
    public void RssChannel_ShouldSerializeToValidXml()
    {
        // Arrange
        var rssChannel = new RssChannel
        {
            Channel = new Channel
            {
                Title = "Test RSS",
                Link = "https://example.com",
                Description = "Test Description",
                LastBuildDate = DateTime.UtcNow.ToString("R"),
                Items = new List<RssItem>
                {
                    new RssItem
                    {
                        Title = "Test Item",
                        Link = "https://example.com/item1",
                        Description = "Test item description",
                        PubDate = DateTime.UtcNow.ToString("R"),
                        Guid = "test-guid-1",
                        Category = "Test"
                    }
                }
            }
        };

        // Act
        var xmlSerializer = new XmlSerializer(typeof(RssChannel));
        var stringBuilder = new StringBuilder();

        using (var writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            OmitXmlDeclaration = false
        }))
        {
            xmlSerializer.Serialize(writer, rssChannel);
        }

        var xml = stringBuilder.ToString();

        // Assert
        Assert.NotEmpty(xml);
        Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-16\"?>", xml);
        Assert.Contains("version=\"2.0\"", xml);
        Assert.Contains("<channel>", xml);
        Assert.Contains("<title>Test RSS</title>", xml);
        Assert.Contains("<item>", xml);
        Assert.Contains("<title>Test Item</title>", xml);
    }
}