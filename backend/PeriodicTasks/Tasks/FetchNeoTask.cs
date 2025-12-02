using System.Text.Json;
using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchNeoTask : IPeriodicTask
{
    public string Name => "fetch_neo";
    public TimeSpan Interval { get; }
    public string Url { get; }

    private readonly ApiSender _apiSender;
    private readonly ISpaceRepository _repo;
    private readonly SpaceOptions _opts;

    public FetchNeoTask(ISpaceRepository repo, IOptions<SpaceOptions> opts, ApiSender apiSender)
    {
        _repo = repo;
        _opts = opts.Value;
        Interval = TimeSpan.FromSeconds(_opts.NeoInterval);
        Url = _opts.NeoUrl;
        _apiSender = apiSender;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        JsonDocument? doc = await _apiSender.Send(Url, stoppingToken);
        if (doc != null) await _repo.InsertSpaceCacheAsync("neo", doc);
    }
}
