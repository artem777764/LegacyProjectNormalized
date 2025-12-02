using System.Text.Json;
using System.Threading.Tasks;
using backend.DTOs.IssDTOs;
using backend.Models.Entities;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class IssController : ControllerBase
{
    private readonly IIssRepository _issRepository;
    private FetchIssTask _task;

    public IssController(IIssRepository issRepository, FetchIssTask task)
    {
        _issRepository = issRepository;
        _task = task;
    }
    
    [HttpGet("last")]
    public async Task<IActionResult> GetLastRecord()
    {
        IssFetchLogEntity issRecord = await _issRepository.GetLastRecord();
        return Ok(new GetIssDTO
        {
            Id = issRecord.Id,
            Fetched_at = issRecord.FetchedAt,
            Source_url = issRecord.SourceUrl,
            Payload = issRecord.Payload,
        });
    }

    [HttpGet("fetch")]
    public async Task<IActionResult> Fetch()
    {
        await _task.ExecuteAsync(CancellationToken.None);
        
        IssFetchLogEntity issRecord = await _issRepository.GetLastRecord();
        return Ok(new GetIssDTO
        {
            Id = issRecord.Id,
            Fetched_at = issRecord.FetchedAt,
            Source_url = issRecord.SourceUrl,
            Payload = issRecord.Payload,
        });
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend()
    {
        var lastTwo = await _issRepository.GetLastNRecords(2);

        if (lastTwo.Count < 2)
        {
            return Ok(new IssTrendDTO
            {
                Movement = false,
                DeltaKm = 0,
                DtSec = 0,
                VelocityKmh = null,
                FromTime = null,
                ToTime = null,
                FromLat = null,
                FromLon = null,
                ToLat = null,
                ToLon = null
            });
        }

        var r1 = lastTwo[1];
        var r2 = lastTwo[0];

        double? lat1 = GetDoubleFromJson(r1.Payload, "latitude");
        double? lon1 = GetDoubleFromJson(r1.Payload, "longitude");
        double? lat2 = GetDoubleFromJson(r2.Payload, "latitude");
        double? lon2 = GetDoubleFromJson(r2.Payload, "longitude");
        double? v2 = GetDoubleFromJson(r2.Payload, "velocity");

        double deltaKm = 0;
        bool movement = false;

        if (lat1.HasValue && lon1.HasValue && lat2.HasValue && lon2.HasValue)
        {
            deltaKm = HaversineKm(lat1.Value, lon1.Value, lat2.Value, lon2.Value);
            movement = deltaKm > 0.1;
        }

        double dtSec = (r2.FetchedAt - r1.FetchedAt).TotalSeconds;

        return Ok(new IssTrendDTO
        {
            Movement = movement,
            DeltaKm = deltaKm,
            DtSec = dtSec,
            VelocityKmh = v2,
            FromTime = r1.FetchedAt,
            ToTime = r2.FetchedAt,
            FromLat = lat1,
            FromLon = lon1,
            ToLat = lat2,
            ToLon = lon2
        });
    }

    private double? GetDoubleFromJson(string json, string key)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(key, out var val))
            {
                if (val.ValueKind == JsonValueKind.Number) return val.GetDouble();
                if (val.ValueKind == JsonValueKind.String && double.TryParse(val.GetString(), out var d)) return d;
            }
        }
        catch { }
        return null;
    }

    private double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371;
        double dLat = ToRad(lat2 - lat1);
        double dLon = ToRad(lon2 - lon1);
        double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon/2) * Math.Sin(dLon/2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
        return R * c;
    }

    private double ToRad(double deg) => deg * Math.PI / 180.0;
}
