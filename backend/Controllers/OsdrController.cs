using System.Text.Json;
using System.Threading.Tasks;
using backend.DTOs.IssDTOs;
using backend.DTOs.OsdrDTOs;
using backend.Models.Entities;
using backend.PeriodicTasks;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpaceApp.Options;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class OsdrController : ControllerBase
{
    private readonly IOsdrRepository _osdrRepository;
    private readonly ApiSender _apiSender;
    private readonly IOptions<SpaceOptions> _opts;

    public OsdrController(IOsdrRepository osdrRepository, ApiSender apiSender, IOptions<SpaceOptions> opts)
    {
        _osdrRepository = osdrRepository;
        _apiSender = apiSender;
        _opts = opts;
    }
    
    [HttpGet("sync")]
    public async Task<IActionResult> Sync()
    {
        var doc = await _apiSender.Send(_opts.Value.OsdrDatasetUrl);
        int written = 0;

        if (doc != null) written = await _osdrRepository.SaveOsdrItemsAsync(doc);
        return Ok(new GetCountOsdrDTO
        {
            Written = written,
        });
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetLastRecords()
    {
        List<OsdrItemEntity> osdrItemEntity = await _osdrRepository.GetLastNRecords(20);
        return Ok(osdrItemEntity.Select(o => new GetOsdrDTO
        {
            Id = o.Id,
            DatasetId = o.DatasetId,
            Title = o.Title,
            Status = o.Status,
            FetchedAt = o.InsertedAt,
            UpdatedAt = o.UpdatedAt,
            Payload = o.Raw,
        }));
    }
}
