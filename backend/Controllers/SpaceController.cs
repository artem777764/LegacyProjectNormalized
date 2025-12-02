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

    public SpaceController(ISpaceRepository spaceRepository)
    {
        _spaceRepository = spaceRepository;
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
}
