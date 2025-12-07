using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JwstController : ControllerBase
    {
        private readonly string _apiKey;
        private readonly string? _email;
        private const string JWST_HOST = "https://api.jwstapi.com";

        public JwstController(IConfiguration cfg)
        {
            _apiKey = cfg["JWST_API_KEY"] ?? throw new InvalidOperationException("JWST_API_KEY is missing");
            _email = cfg["JWST_EMAIL"];
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed(
            [FromQuery] string source = "jpg",
            [FromQuery] string? suffix = null,
            [FromQuery] string? program = null,
            [FromQuery] string? instrument = null,
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 24,
            CancellationToken ct = default)
        {
            try
            {
                string path = source.ToLower() switch
                {
                    "suffix" when !string.IsNullOrWhiteSpace(suffix) => $"all/suffix/{Uri.EscapeDataString(suffix)}",
                    "program" when !string.IsNullOrWhiteSpace(program) => $"program/id/{Uri.EscapeDataString(program)}",
                    _ => "all/type/jpg"
                };

                var url = $"{JWST_HOST}/{path}?page={page}&perPage={perPage}";

                using var client = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.Add("X-API-KEY", _apiKey);
                if (!string.IsNullOrEmpty(_email))
                    request.Headers.Add("email", _email);

                using var response = await client.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(ct);
                var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

                var root = jsonDoc.RootElement;
                var items = new List<object>();

                if (root.TryGetProperty("body", out var body))
                {
                    foreach (var it in body.EnumerateArray())
                    {
                        if (!it.ValueKind.HasFlag(JsonValueKind.Object)) continue;

                        var instList = new List<string>();
                        if (it.TryGetProperty("details", out var details) &&
                            details.TryGetProperty("instruments", out var instruments))
                        {
                            foreach (var i in instruments.EnumerateArray())
                            {
                                if (i.TryGetProperty("instrument", out var instr))
                                    instList.Add(instr.GetString()?.ToUpper() ?? "");
                            }
                        }

                        if (!string.IsNullOrEmpty(instrument) && instList.Any() &&
                            !instList.Contains(instrument.ToUpper())) continue;

                        string? urlLink = null;
                        if (it.TryGetProperty("url", out var urlProp)) urlLink = urlProp.GetString();
                        if (string.IsNullOrEmpty(urlLink) && it.TryGetProperty("location", out var locProp))
                            urlLink = locProp.GetString();
                        if (string.IsNullOrEmpty(urlLink)) continue;

                        string obsId = GetJsonString(it, "observation_id");
                        string prog = GetJsonString(it, "program");
                        string sfx = it.TryGetProperty("details", out var det) && det.TryGetProperty("suffix", out var sfxProp)
                            ? sfxProp.GetString() ?? ""
                            : "";

                        items.Add(new
                        {
                            url = urlLink,
                            obs = obsId,
                            program = prog,
                            suffix = sfx,
                            inst = instList,
                            caption = $"{obsId} · P{prog}" + (string.IsNullOrEmpty(sfx) ? "" : $" · {sfx}") +
                                      (instList.Any() ? $" · {string.Join("/", instList)}" : ""),
                            link = urlLink
                        });

                        if (items.Count >= perPage) break;
                    }
                }

                return Ok(new
                {
                    source = path,
                    count = items.Count,
                    items
                });
            }
            catch (HttpRequestException hre)
            {
                return StatusCode(502, new { error = $"HTTP error: {hre.Message}" });
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return StatusCode(499, new { error = "Запрос отменён" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private static string GetJsonString(JsonElement el, string propName)
        {
            if (!el.TryGetProperty(propName, out var prop)) return "";
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString() ?? "",
                JsonValueKind.Number => prop.GetRawText(),
                _ => ""
            };
        }
    }
}
