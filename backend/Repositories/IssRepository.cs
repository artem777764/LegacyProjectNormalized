using System.Text.Json;
using backend.Models;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class IssRepository : IIssRepository
{
    private readonly ApplicationDbContext _context;

    public IssRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task InsertIssDataAsync(string sourceUrl, JsonDocument payload)
    {
        IssFetchLogEntity item = new IssFetchLogEntity
        {
            SourceUrl = sourceUrl,
            Payload = payload.RootElement.GetRawText()
        };

        _context.IssFetchLogs.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task<IssFetchLogEntity> GetLastRecord()
    {
        return _context.IssFetchLogs.Order().AsNoTracking().Last();
    }
}