using ApiAggregator.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace ApiAggregator.Services
{
    public class NewsService
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly IMemoryCache _cache;
        private readonly IRestClientWrapper _restClientWrapper;

        public NewsService(IConfiguration configuration, IMemoryCache cache, IRestClientWrapper restClientWrapper)
        {
            _baseUrl = configuration["NewsAPI:BaseUrl"];
            _apiKey = configuration["NewsAPI:ApiKey"];
            _cache = cache;
            _restClientWrapper = restClientWrapper;

            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new Exception("Base URL for NewsAPI is not configured.");
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("API Key for NewsAPI is not configured.");
            }
        }

        public async Task<List<Article>> GetNewsAsync(string keyword = "general")
        {
            keyword = string.IsNullOrWhiteSpace(keyword) ? "general" : keyword.Trim().ToLower();
            string cacheKey = $"NewsCache_{keyword}";

            if (_cache.TryGetValue(cacheKey, out List<Article> cachedArticles))
            {
                return cachedArticles;
            }

            try
            {
                var newsData = await FetchNewsDataAsync(keyword);

                if (newsData == null || newsData.Articles == null || !newsData.Articles.Any())
                {
                    Console.WriteLine($"No valid data for keyword: {keyword}. Falling back to 'general'.");
                    return GetStaticFallbackArticles();
                }

                _cache.Set(cacheKey, newsData.Articles, TimeSpan.FromMinutes(10));

                return newsData.Articles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching news for keyword: {keyword}. {ex.Message}");
                return GetStaticFallbackArticles();
            }
        }

        private async Task<NewsModel> FetchNewsDataAsync(string keyword)
        {
            try
            {
                var request = new RestRequest($"{_baseUrl}/everything?q={keyword}&apiKey={_apiKey}&sortBy=publishedAt&pageSize=10", Method.Get);
                var response = await _restClientWrapper.ExecuteAsync(request);

                if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
                {
                    Console.WriteLine($"Failed to fetch news for keyword: {keyword}. Status: {response?.StatusCode}");
                    return null;
                }

                return JsonSerializer.Deserialize<NewsModel>(response.Content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching news for keyword: {keyword}. {ex.Message}");
                return null;
            }
        }

        private List<Article> GetStaticFallbackArticles()
        {
            return new List<Article>
            {
                new Article
                {
                    Title = "No news available",
                    Description = "We are unable to fetch news at this time. Please try again later.",
                    Url = string.Empty
                }
            };
        }
    }
}
