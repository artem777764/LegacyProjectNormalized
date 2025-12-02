using System.Text.Json;
using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchOsdrTask : IPeriodicTask
{
    public string Name => "fetch_osdr";
    public TimeSpan Interval { get; }
    public string Url { get; }

    private readonly ISpaceRepository _repoSpace;
    private readonly IOsdrRepository _repoOsdr;
    private readonly SpaceOptions _opts;
    private readonly ApiSender _apiSender;

    public FetchOsdrTask(ISpaceRepository repoSpace, IOsdrRepository repoOsdr, IOptions<SpaceOptions> opts, ApiSender apiSender)
    {
        _repoSpace = repoSpace;
        _repoOsdr = repoOsdr;
        _opts = opts.Value;
        _apiSender = apiSender;

        Interval = TimeSpan.FromSeconds(Math.Max(1, _opts.OsdrInterval));
        Url = _opts.OsdrDatasetUrl;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        JsonDocument? doc = await _apiSender.Send(Url, stoppingToken);
        if (doc != null) await _repoSpace.InsertSpaceCacheAsync("osdr", doc);
        if (doc != null) await _repoOsdr.SaveOsdrItemsAsync(doc);
    }
}
