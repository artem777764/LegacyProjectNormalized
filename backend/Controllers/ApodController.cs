using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using SpaceApp.Options;
using backend.PeriodicTasks;

[ApiController]
[Route("api/[controller]")]
public class ApodController : ControllerBase
{
    private const string RedisKey = "apod";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(3600);

    private readonly IDatabase _redis;
    private readonly ApiSender _apiSender;
    private readonly SpaceOptions _opts;

    public ApodController(
        IConnectionMultiplexer redis,
        ApiSender apiSender,
        IOptions<SpaceOptions> opts)
    {
        _redis = redis.GetDatabase();
        _apiSender = apiSender;
        _opts = opts.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        RedisValue cached = await _redis.StringGetAsync(RedisKey);
        if (cached.HasValue)
        {
            return Content(cached!, "application/json");
        }

        JsonDocument? doc = await _apiSender.Send(_opts.ApodUrl, cancellationToken);
        if (doc == null)
        {
            return StatusCode(502, "Failed to fetch APOD data");
        }

        string json = doc.RootElement.GetRawText();

        await _redis.StringSetAsync(
            RedisKey,
            json,
            CacheTtl
        );

        return Content(json, "application/json");
    }
}