using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpPost("random")]
        public async Task<IActionResult> AddRandom()
        {
            var db = _redis.GetDatabase();

            var id = Guid.NewGuid().ToString("N");
            var value = new
            {
                Value = new Random().Next(0, 1000000),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var key = $"random:{id}";
            var json = System.Text.Json.JsonSerializer.Serialize(value);

            await db.StringSetAsync(key, json, TimeSpan.FromSeconds(60));

            return CreatedAtAction(nameof(GetAll), new { key }, new { key, value });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var db = _redis.GetDatabase();
            var endpoints = _redis.GetEndPoints();
            if (endpoints.Length == 0)
                return StatusCode(500, "No redis endpoints available");

            var server = _redis.GetServer(endpoints[0]);

            var keys = server.Keys(pattern: "random:*").ToArray();

            var result = new List<object>();
            foreach (var key in keys)
            {
                var val = await db.StringGetAsync(key);
                if (val.HasValue)
                {
                    try
                    {
                        var des = System.Text.Json.JsonSerializer.Deserialize<object>(val!);
                        result.Add(new { key = key.ToString(), value = des });
                    }
                    catch
                    {
                        result.Add(new { key = key.ToString(), value = val.ToString() });
                    }
                }
            }

            return Ok(result);
        }
    }
}
