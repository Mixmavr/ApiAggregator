using ApiAggregator.Models;
using ApiAggregator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using RestSharp;
using System.Net;
using System.Text.Json;
using Xunit;

public class WeatherServiceTests
{
    [Fact]
    public async Task GetWeatherAsync_ReturnsWeatherData_WhenCityIsValid()
    {
        var city = "Athens";
        var weatherData = new WeatherModel
        {
            Name = "Athens",
            Main = new MainWeather { Temp = 20.5 }
        };

        var responseContent = JsonSerializer.Serialize(weatherData);

        var mockRestClientWrapper = new Mock<IRestClientWrapper>();
        mockRestClientWrapper
            .Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                Content = responseContent
            });

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["OpenWeatherMap:BaseUrl"]).Returns("https://api.openweathermap.org/data/2.5");
        mockConfig.Setup(c => c["OpenWeatherMap:ApiKey"]).Returns("fake_api_key");

        var mockCache = new Mock<IMemoryCache>();

        var cacheEntryMock = new Mock<ICacheEntry>();
        mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        var weatherService = new WeatherService(mockConfig.Object, mockCache.Object, mockRestClientWrapper.Object);

        var result = await weatherService.GetWeatherAsync(city);

        Assert.NotNull(result);
        Assert.Equal("Athens", result.Name);
        Assert.Equal(20.5, result.Main.Temp);

        mockRestClientWrapper.Verify(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
