using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchOsdrTask : IPeriodicTask
{
    public string Name => "fetch_osdr";
    public TimeSpan Interval { get; }
    public string Url { get; }

    private readonly IHttpService _httpClient;
    private readonly ISpaceRepository _repoSpace;
    private readonly IOsdrRepository _repoOsdr;
    private readonly SpaceOptions _opts;

    public FetchOsdrTask(IHttpService httpClient, ISpaceRepository repoSpace, IOsdrRepository repoOsdr, IOptions<SpaceOptions> opts)
    {
        _httpClient = httpClient;
        _repoSpace = repoSpace;
        _repoOsdr = repoOsdr;
        _opts = opts.Value;

        Interval = TimeSpan.FromSeconds(Math.Max(1, _opts.OsdrInterval));
        Url = _opts.OsdrDatasetUrl;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Console.WriteLine($"{Name}: fetching {Url}");
            using var doc = await _httpClient.GetJsonDocumentAsync(Url, stoppingToken);
            var raw = doc.RootElement.GetRawText();
            await _repoSpace.InsertSpaceCacheAsync("osdr", doc);
            await _repoOsdr.SaveOsdrItemsAsync(doc);
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
