using System.Text.Json;
using System.Threading.Tasks;
using backend.DTOs.IssDTOs;
using backend.DTOs.SpaceDTOs;
using backend.Models.Entities;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class SpaceController : ControllerBase
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IOsdrRepository _osdrRepository;
    private readonly FetchApodTask _fetchApodTask;
    private readonly FetchCmeTask _fetchCmeTask;
    private readonly FetchOsdrTask _fetchOsdrTask;
    private readonly FetchFlrTask _fetchFlrTask;
    private readonly FetchIssTask _fetchIssTask;
    private readonly FetchNeoTask _fetchNeoTask;
    private readonly FetchSpacexTask _fetchSpacexTask;

    private static readonly List<string> Sources = new List<string> { "apod", "neo", "flr", "cme", "spacex", "osdr", "iss" };

    public SpaceController(
        ISpaceRepository spaceRepository,
        IOsdrRepository osdrRepository,
        FetchApodTask fetchApodTask,
        FetchCmeTask fetchCmeTask,
        FetchOsdrTask fetchOsdrTask,
        FetchFlrTask fetchFlrTask,
        FetchIssTask fetchIssTask,
        FetchNeoTask fetchNeoTask,
        FetchSpacexTask fetchSpacexTask
    )
    {
        _spaceRepository = spaceRepository;
        _osdrRepository = osdrRepository;
        _fetchApodTask = fetchApodTask;
        _fetchCmeTask = fetchCmeTask;
        _fetchOsdrTask = fetchOsdrTask;
        _fetchFlrTask = fetchFlrTask;
        _fetchIssTask = fetchIssTask;
        _fetchNeoTask = fetchNeoTask;
        _fetchSpacexTask = fetchSpacexTask;
    }
    
    [HttpGet("{source}/latest")]
    public async Task<IActionResult> GetLastRecord([FromRoute] string source)
    {
        SpaceCacheEntity? spaceRecord = await _spaceRepository.GetLastRecordBySource(source);
        if (spaceRecord == null)
        {
            return Ok(new GetSpaceNoDateDTO
            {
                Source = source ?? "null",
                Message = "No data",
            });
        }

        return Ok(new GetSpaceDTO
        {
            Source = source!,
            Fetched_at = spaceRecord.FetchedAt,
            Payload = spaceRecord.Payload,
        });
    }

    [HttpGet("refresh")]
    public async Task<IActionResult> Refresh([FromQuery] string src = "apod,neo,flr,cme,spacex")
    {
        List<string> options = src.Split(",").ToList();
        List<string> refreshed = new List<string>();

        foreach (string option in options)
        {
            if (option == "apod") await _fetchApodTask.ExecuteAsync(CancellationToken.None);
            else if (option == "neo") await _fetchNeoTask.ExecuteAsync(CancellationToken.None);
            else if (option == "flr") await _fetchFlrTask.ExecuteAsync(CancellationToken.None);
            else if (option == "cme") await _fetchCmeTask.ExecuteAsync(CancellationToken.None);
            else if (option == "spacex") await _fetchSpacexTask.ExecuteAsync(CancellationToken.None);
            else if (option == "iss") await _fetchIssTask.ExecuteAsync(CancellationToken.None);
            else if (option == "osdr") await _fetchOsdrTask.ExecuteAsync(CancellationToken.None);
            else
            {
                continue;
            }
            refreshed.Add(option);
        }

        return Ok(refreshed);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var result = new SpaceSummaryDTO();

        foreach (string source in Sources)
        {
            SpaceCacheEntity? lastRecord = await _spaceRepository.GetLastRecordBySource(source);
            if (lastRecord != null)
            {
                result.Items[source] = new GetSpaceItemSummaryDTO
                {
                    FetchedAt = lastRecord.FetchedAt,
                    Payload = lastRecord.Payload,
                };
            }
        }

        result.osdrCount = await _osdrRepository.GetDatasetsCount();
        return Ok(result);
    }
}
