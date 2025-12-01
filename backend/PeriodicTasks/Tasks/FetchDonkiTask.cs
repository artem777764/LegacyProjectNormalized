using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchDonkiTask : IPeriodicTask
{
    public string Name => "fetch_donki";
    public TimeSpan Interval { get; }
    public string Url { get; }

    private readonly IHttpService _httpClient;
    private readonly ISpaceRepository _repo;
    private readonly SpaceOptions _opts;

    private const int DaysRange = 5;

    public FetchDonkiTask(IHttpService httpClient, ISpaceRepository repo, IOptions<SpaceOptions> opts)
    {
        _httpClient = httpClient;
        _repo = repo;
        _opts = opts.Value;
        Interval = TimeSpan.FromSeconds(Math.Max(1, _opts.DonkiInterval));
        Url = _opts.DonkiUrl;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-DaysRange).ToString("yyyy-MM-dd");
            var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var url = $"{Url}?startDate={startDate}&endDate={endDate}";
            url += $"&api_key={_opts.NasaApiKey}";

            Console.WriteLine($"{Name}: fetching {url}");

            using var doc = await _httpClient.GetJsonDocumentAsync(url, stoppingToken);
            var raw = doc.RootElement.GetRawText();
            await _repo.InsertSpaceCacheAsync("donki", doc);
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