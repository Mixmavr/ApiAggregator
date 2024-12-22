namespace ApiAggregator.Services
{
    using ApiAggregator.Models;
    using Microsoft.Extensions.Caching.Memory;
    using RestSharp;
    using System.Diagnostics;
    using System.Net;
    using System.Text.Json;

    public class WeatherService
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly IMemoryCache _cache;
        private readonly IRestClientWrapper _restClientWrapper;

        public WeatherService(
            IConfiguration configuration,
            IMemoryCache cache,
            IRestClientWrapper restClientWrapper
            )
        {
            _baseUrl = configuration["OpenWeatherMap:BaseUrl"];
            _apiKey = configuration["OpenWeatherMap:ApiKey"];
            _cache = cache;
            _restClientWrapper = restClientWrapper;

            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new Exception("Base URL for OpenWeatherMap is not configured.");
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("API Key for OpenWeatherMap is not configured.");
            }
        }

        public async Task<WeatherModel> GetWeatherAsync(string city)
        {
            city = string.IsNullOrWhiteSpace(city) ? "Athens" : city.Trim();
            string cacheKey = $"WeatherCache_{city}";

            if (_cache.TryGetValue(cacheKey, out WeatherModel cachedWeather))
            {
                return cachedWeather;
            }

            try
            {
                var request = new RestRequest($"{_baseUrl}/weather?q={city}&appid={_apiKey}&units=metric", Method.Get);

                var response = await _restClientWrapper.ExecuteAsync(request);

                if (response == null || response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
                {
                    throw new HttpRequestException($"Failed to fetch weather: {response?.StatusCode} - {response?.Content}");
                }

                var weatherData = JsonSerializer.Deserialize<WeatherModel>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (weatherData == null)
                {
                    throw new JsonException("Invalid weather data received.");
                }

                _cache.Set(cacheKey, weatherData, TimeSpan.FromMinutes(30));

                return weatherData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching weather data: {ex.Message}");
            }
        }
    }
}
