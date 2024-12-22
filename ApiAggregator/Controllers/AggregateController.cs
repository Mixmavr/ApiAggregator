using ApiAggregator.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/aggregate")]
public class AggregateController : ControllerBase
{
    private readonly WeatherService _weatherService;
    private readonly NewsService _newsService;
    private readonly GitHubService _githubService;

    public AggregateController(WeatherService weatherService, NewsService newsService, GitHubService githubService)
    {
        _weatherService = weatherService;
        _newsService = newsService;
        _githubService = githubService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAggregateData([FromQuery] string city="Athens", [FromQuery] string keyword = "General", [FromQuery] string owner = "Mixmavr")
    {
        try
        {
            var weatherTask = _weatherService.GetWeatherAsync(city);
            var newsTask = _newsService.GetNewsAsync(keyword);
            var reposTask = _githubService.GetReposAsync(owner);

            await Task.WhenAll(weatherTask, newsTask, reposTask);

            var result = new
            {
                Weather = await weatherTask,
                News = await newsTask,
                Repositories = await reposTask
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}
