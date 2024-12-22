using ApiAggregator.Models;
using ApiAggregator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using RestSharp;
using System.Net;
using System.Text.Json;
using Xunit;

public class GitHubServiceTests
{
    [Fact]
    public async Task GetOwnerReposTest()
    {
        var username = "testUser";

        var repositories = new List<RepositoryModel>
    {
        new RepositoryModel
        {
            Id = 1,
            Name = "TestRepo",
            Description = "Test Repository",
            Owner = new Owner { Url = "https://example.com" }
        }
    };

        var mockRestClientWrapper = new Mock<IRestClientWrapper>();
        mockRestClientWrapper
            .Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                Content = JsonSerializer.Serialize(repositories),
                ErrorMessage = null,
                ErrorException = null
            });

        var cache = new MemoryCache(new MemoryCacheOptions());
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["GitHubAPI:AccessToken"]).Returns("fake_token");
        configMock.Setup(c => c["GitHubAPI:BaseUrl"]).Returns("https://api.github.com");

        var gitHubService = new GitHubService(configMock.Object, cache, mockRestClientWrapper.Object);

        var result = await gitHubService.GetOwnerReposAsync(username);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("TestRepo", result[0].Name);
    }

}
