using Microsoft.AspNetCore.Mvc;

namespace Neoron.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class WeatherForecastController : ControllerBase
{

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult<IEnumerable<WeatherForecast>> Get()
    {
        try
        {
            _logger.LogInformation("Weather forecast endpoint called, but no data is available.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the weather forecast.");
            return StatusCode(500, "Internal server error");
        }
    }
}
