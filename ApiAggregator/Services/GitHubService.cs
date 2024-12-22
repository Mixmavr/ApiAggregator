using ApiAggregator.Models;
using Microsoft.Extensions.Caching.Memory;
using RestSharp;
using System.Net;
using System.Text.Json;

public class GitHubService
{
    private readonly string _baseUrl;
    private readonly string _accessToken;
    private readonly IMemoryCache _cache;
    private readonly IRestClientWrapper _restClientWrapper;

    public GitHubService(IConfiguration configuration, IMemoryCache cache, IRestClientWrapper restClientWrapper)
    {
        _baseUrl = configuration["GitHubAPI:BaseUrl"];
        _accessToken = configuration["GitHubAPI:AccessToken"];
        _cache = cache;
        _restClientWrapper = restClientWrapper;

        if (string.IsNullOrEmpty(_baseUrl))
        {
            throw new Exception("Base URL for GitHubAPI is not configured.");
        }

        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new Exception("Access Token for GitHubAPI is not configured.");
        }
    }

    public async Task<List<RepositoryModel>> GetReposAsync(string username)
    {
        username = string.IsNullOrWhiteSpace(username) ? "Mixmavr" : username.Trim().ToLower();
        string cacheKey = $"GitHubReposCache_{username}";

        if (_cache.TryGetValue(cacheKey, out List<RepositoryModel> cachedRepos))
        {
            return cachedRepos;
        }

        try
        {
            var repositories = await FetchRepositoriesAsync(username);

            if (repositories == null || !repositories.Any())
            {
                Console.WriteLine($"No repositories found for username: {username}. Returning static fallback data.");
                return GetStaticFallbackRepositories(username);
            }

            _cache.Set(cacheKey, repositories, TimeSpan.FromMinutes(60));

            return repositories;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching repositories for username: {username}. {ex.Message}. Returning static fallback data.");
            return GetStaticFallbackRepositories(username);
        }
    }

    private async Task<List<RepositoryModel>> FetchRepositoriesAsync(string username)
    {
        try
        {
            var request = new RestRequest($"/users/{username}/repos", Method.Get);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");

            var response = await _restClientWrapper.ExecuteAsync(request);

            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                Console.WriteLine($"Failed to fetch repositories for username: {username}. Status: {response?.StatusCode}");
                return null;
            }

            return JsonSerializer.Deserialize<List<RepositoryModel>>(response.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching repositories for username: {username}. {ex.Message}");
            return null;
        }
    }

    private List<RepositoryModel> GetStaticFallbackRepositories(string username)
    {
        return new List<RepositoryModel>
        {
            new RepositoryModel
            {
                Name = "No repositories available",
                Description = "We are unable to fetch repositories at this time. Please try again later.",
                Owner = new Owner
                {
                    Url = string.Empty
                }
            }
        };
    }
}
