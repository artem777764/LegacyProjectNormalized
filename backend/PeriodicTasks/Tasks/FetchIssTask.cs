using System.Text.Json;
using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

public class FetchIssTask : IPeriodicTask
{
    public string Name => "fetch_iss";
    public TimeSpan Interval { get; }
    public string Url { get; }

    private readonly IIssRepository _issRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly SpaceOptions _opts;
    private readonly ApiSender _apiSender;

    public FetchIssTask(IIssRepository issRepository, ISpaceRepository spaceRepository, IOptions<SpaceOptions> opts, ApiSender apiSender)
    {
        _issRepository = issRepository;
        _spaceRepository = spaceRepository;
        _opts = opts.Value;
        _apiSender = apiSender;

        Interval = TimeSpan.FromSeconds(Math.Max(1, _opts.IssInterval));
        Url = _opts.IssUrl;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        JsonDocument? doc = await _apiSender.Send(Url, stoppingToken);
        if (doc != null) await _issRepository.InsertIssDataAsync(Url, doc);
        if (doc != null) await _spaceRepository.InsertSpaceCacheAsync("iss", doc);
    }
}