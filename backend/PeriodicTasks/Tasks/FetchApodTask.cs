using System.Text.Json;
using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchApodTask : IPeriodicTask
{
    public string Name => "fetch_apod";
    public string Url { get; }
    public TimeSpan Interval { get; }

    private readonly ISpaceRepository _repo;
    private readonly SpaceOptions _opts;
    private readonly ApiSender _apiSender;

    public FetchApodTask(ISpaceRepository repo, IOptions<SpaceOptions> opts, ApiSender apiSender)
    {
        _repo = repo;
        _opts = opts.Value;
        _apiSender = apiSender;
        Interval = TimeSpan.FromSeconds(_opts.ApodInterval);
        Url = _opts.ApodUrl;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        JsonDocument? doc = await _apiSender.Send(Url, stoppingToken);
        if (doc != null) await _repo.InsertSpaceCacheAsync("apod", doc);
    }
}
