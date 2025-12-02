using System.Text.Json;
using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchFlrTask : IPeriodicTask
{
    public string Name => "fetch_flr";
    public TimeSpan Interval { get; }
    public string Url { get; }

    private readonly ISpaceRepository _repo;
    private readonly SpaceOptions _opts;
    private readonly ApiSender _apiSender;

    public FetchFlrTask(ISpaceRepository repo, IOptions<SpaceOptions> opts, ApiSender apiSender)
    {
        _repo = repo;
        _opts = opts.Value;
        _apiSender = apiSender;
        Interval = TimeSpan.FromSeconds(Math.Max(1, _opts.DonkiInterval));
        Url = _opts.DonkiFLR;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        JsonDocument? doc = await _apiSender.Send(Url, stoppingToken);
        if (doc != null) await _repo.InsertSpaceCacheAsync("flr", doc);
    }
}