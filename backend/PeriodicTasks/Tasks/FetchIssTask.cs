using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchIssTask : IPeriodicTask
{
    public string Name => "fetch_iss";
    public TimeSpan Interval { get; }
    public string Url { get; }

    private readonly IHttpService _httpClient;
    private readonly ISpaceRepository _repo;
    private readonly SpaceOptions _opts;

    public FetchIssTask(IHttpService httpClient, ISpaceRepository repo, IOptions<SpaceOptions> opts)
    {
        _httpClient = httpClient;
        _repo = repo;
        _opts = opts.Value;

        Interval = TimeSpan.FromSeconds(Math.Max(1, _opts.IssInterval));
        Url = _opts.IssUrl;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Console.WriteLine($"{Name}: fetching {Url}");
            using var doc = await _httpClient.GetJsonDocumentAsync(Url, stoppingToken);
            var raw = doc.RootElement.GetRawText();
            await _repo.InsertSpaceCacheAsync("iss", doc);
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