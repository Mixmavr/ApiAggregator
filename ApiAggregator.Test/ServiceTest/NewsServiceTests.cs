using ApiAggregator.Models;
using ApiAggregator.Services;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Xunit;
using RestSharp;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Collections.Generic;

public class NewsServiceTests
{
    [Fact]
    public async Task GetNewsTest()
    {
        var keyword = "general";

        var mockArticles = new List<Article>
        {
            new Article { Title = "Test Article", Description = "Test Description", Url = "https://example.com" }
        };

        var mockResponse = new RestResponse
        {
            StatusCode = HttpStatusCode.OK,
            ResponseStatus = ResponseStatus.Completed,
            Content = JsonSerializer.Serialize(new NewsModel { Articles = mockArticles })
        };

        var mockRestClientWrapper = new Mock<IRestClientWrapper>();
        mockRestClientWrapper
            .Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["NewsAPI:BaseUrl"]).Returns("https://newsapi.org/v2");
        mockConfig.Setup(c => c["NewsAPI:ApiKey"]).Returns("fake_api_key");

        var mockCache = new Mock<IMemoryCache>();
        object cacheEntry = null;

        mockCache
            .Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheEntry))
            .Returns(false);

        var mockCacheEntry = new Mock<ICacheEntry>();
        mockCache
            .Setup(c => c.CreateEntry(It.IsAny<object>()))
            .Returns(mockCacheEntry.Object);

        var newsService = new NewsService(mockConfig.Object, mockCache.Object, mockRestClientWrapper.Object);

        var result = await newsService.GetNewsAsync(keyword);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Article", result[0].Title);

        mockRestClientWrapper.Verify(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
