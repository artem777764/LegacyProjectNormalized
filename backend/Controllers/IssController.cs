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
}
