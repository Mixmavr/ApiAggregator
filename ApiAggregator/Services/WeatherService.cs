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
                var weatherData = await FetchWeatherDataAsync(city);

                if (weatherData == null)
                {
                    Debug.WriteLine($"No valid weather data for city: {city}. Returning fallback weather data.");
                    return GetStaticFallbackWeather(city);
                }
                _cache.Set(cacheKey, weatherData, TimeSpan.FromMinutes(30));
                return weatherData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather data for city: {city}. {ex.Message}. Returning fallback weather data.");
                return GetStaticFallbackWeather(city);
            }
        }

        private async Task<WeatherModel> FetchWeatherDataAsync(string city)
        {
            try
            {
                var request = new RestRequest($"{_baseUrl}/weather?q={city}&appid={_apiKey}&units=metric", Method.Get);
                var response = await _restClientWrapper.ExecuteAsync(request);

                if (response == null || response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
                {
                    Debug.WriteLine($"Failed to fetch weather for city: {city}. Status: {response?.StatusCode}");
                    return null;
                }

                return JsonSerializer.Deserialize<WeatherModel>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching weather data for city: {city}. {ex.Message}");
                return null;
            }
        }

        private WeatherModel GetStaticFallbackWeather(string city)
        {
            return new WeatherModel
            {
                Name = "Something went wrong",
                Main = new MainWeather
                {
                    Temp = 20.0, 
                    Feels_Like = 20.0, 
                    Pressure = 1013,
                    Humidity = 50
                }
                
            };
        }
    }
}
