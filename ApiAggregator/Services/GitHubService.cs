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

    public async Task<List<RepositoryModel>> GetOwnerReposAsync(string username)
    {
        username = string.IsNullOrWhiteSpace(username) ? "Mixmavr" : username.Trim().ToLower();
        string cacheKey = $"GitHubReposCache_{username}";

        if (_cache.TryGetValue(cacheKey, out List<RepositoryModel> cachedRepos))
        {
            return cachedRepos;
        }

        try
        {
            var request = new RestRequest($"/users/{username}/repos", Method.Get);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");

            var response = await _restClientWrapper.ExecuteAsync(request);

            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                throw new HttpRequestException($"Failed to fetch repositories: {response.StatusCode} - {response.Content}");
            }

            var repositories = JsonSerializer.Deserialize<List<RepositoryModel>>(response.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (repositories == null || !repositories.Any())
            {
                throw new JsonException("No repositories found or invalid JSON structure.");
            }

            _cache.Set(cacheKey, repositories, TimeSpan.FromMinutes(60));

            return repositories;
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to fetch repositories and no cached data available: {ex.Message}");
        }
    }



}
