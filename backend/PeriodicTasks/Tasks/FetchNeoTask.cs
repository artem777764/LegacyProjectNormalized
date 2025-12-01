using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchNeoTask : IPeriodicTask
{
    public string Name => "fetch_neo";
    public TimeSpan Interval { get; }
    public string Url { get; }

    private readonly IHttpService _httpClient;
    private readonly ISpaceRepository _repo;
    private readonly SpaceOptions _opts;

    public FetchNeoTask(IHttpService httpClient, ISpaceRepository repo, IOptions<SpaceOptions> opts)
    {
        _httpClient = httpClient;
        _repo = repo;
        _opts = opts.Value;
        Interval = TimeSpan.FromSeconds(_opts.NeoInterval);
        Url = _opts.NeoUrl;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var today = DateTime.UtcNow.Date;
        var start = today.AddDays(-2);
        var apiKey = string.IsNullOrWhiteSpace(_opts.NasaApiKey) ? "DEMO_KEY" : _opts.NasaApiKey;
        var url = $"{Url}?start_date={start:yyyy-MM-dd}&end_date={today:yyyy-MM-dd}&api_key={Uri.EscapeDataString(apiKey)}";

        try
        {
            Console.WriteLine($"{Name}: fetching {url}");
            using var doc = await _httpClient.GetJsonDocumentAsync(url, stoppingToken);
            var raw = doc.RootElement.GetRawText();
            await _repo.InsertSpaceCacheAsync("neo", doc);
        }
        catch (HttpRequestException hre)
        {
            Console.WriteLine($"{Name}: HTTP error: {hre.Message}");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"{Name}: cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{Name}: unexpected error: {ex}");
        }
    }
}
