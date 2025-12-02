using System.Text.Json;
using System.Threading.Tasks;
using backend.DTOs.IssDTOs;
using backend.DTOs.OsdrDTOs;
using backend.Models.Entities;
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
    private readonly IHttpClientFactory _factory;
    private readonly IOptions<SpaceOptions> _opts;

    public OsdrController(IOsdrRepository osdrRepository, IHttpClientFactory factory, IOptions<SpaceOptions> opts)
    {
        _osdrRepository = osdrRepository;
        _factory = factory;
        _opts = opts;
    }
    
    [HttpGet("sync")]
    public async Task<IActionResult> Sync()
    {
        var client = _factory.CreateClient();
        using var res = await client.GetAsync(_opts.Value.OsdrDatasetUrl);
        var stream = await res.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        int written = await _osdrRepository.SaveOsdrItemsAsync(doc);
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
            FetchedAt = o.InsertedAt,
            Source = "osdr",
            Payload = o.Raw,
        }));
    }
}
