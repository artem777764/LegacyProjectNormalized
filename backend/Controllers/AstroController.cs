using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AstroController : ControllerBase
    {
        private readonly string _appId;
        private readonly string _secret;
        private const string ASTRO_HOST = "https://api.astronomyapi.com";

        public AstroController(IConfiguration cfg)
        {
            _appId = cfg["ASTRO_APP_ID"]
                ?? throw new InvalidOperationException("ASTRO_APP_ID missing");
            _secret = cfg["ASTRO_APP_SECRET"]
                ?? throw new InvalidOperationException("ASTRO_APP_SECRET missing");
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] string body = "sun",
            [FromQuery] double lat = 55.7558,
            [FromQuery] double lon = 37.6176,
            [FromQuery] int days = 7,
            CancellationToken ct = default)
        {
            days = Math.Clamp(days, 1, 366);

            var fromDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var toDate = DateTime.UtcNow.AddDays(days).ToString("yyyy-MM-dd");

            var query = $"latitude={lat}&longitude={lon}&elevation=0&from_date={fromDate}&to_date={toDate}&time=00:00:00&output=table";

            var url = $"{ASTRO_HOST}/api/v2/bodies/events/{body}?{query}";

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            string authRaw = $"{_appId}:{_secret}";
            string authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authRaw));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authBase64);

            request.Headers.UserAgent.ParseAdd("monolith-iss/1.0");
            request.Headers.Add("Origin", "http://localhost");

            using var response = await client.SendAsync(request, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { error = $"HTTP {(int)response.StatusCode}", raw });
            }

            var json = JsonDocument.Parse(raw);
            return Ok(json.RootElement);
        }
    }
}