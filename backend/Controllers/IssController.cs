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

    public IssController(IIssRepository issRepository)
    {
        _issRepository = issRepository;
    }
    
    [HttpGet("last")]
    public async Task<IActionResult> Get()
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
}
