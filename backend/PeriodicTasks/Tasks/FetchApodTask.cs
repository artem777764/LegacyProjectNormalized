using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchApodTask : IPeriodicTask
{
    public string Name => "fetch_apod";
    public string Url { get; }
    public TimeSpan Interval { get; }

    private readonly IHttpService  _httpClient;
    private readonly ISpaceRepository _repo;
    private readonly SpaceOptions _opts;

    public FetchApodTask(IHttpService httpClient, ISpaceRepository repo, IOptions<SpaceOptions> opts)
    {
        _httpClient = httpClient;
        _repo = repo;
        _opts = opts.Value;
        Interval = TimeSpan.FromSeconds(_opts.ApodInterval);
        Url = _opts.ApodUrl;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var url = _opts.ApodUrl;
        if (!string.IsNullOrEmpty(_opts.NasaApiKey))
            url += "?api_key=" + "DEMO_KEY";
        if (!string.IsNullOrEmpty(_opts.NasaApiKey))
            //url += "?api_key=" + Uri.EscapeDataString(_opts.NasaApiKey);

        try
        {
            using var doc = await _httpClient.GetJsonDocumentAsync(url, stoppingToken);
            await _repo.InsertSpaceCacheAsync("apod", doc);
        }
        catch (HttpRequestException hre)
        {
            Console.WriteLine($"HTTP error fetching APOD: {hre.Message}");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Fetch cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex}");
        }
    }
}
