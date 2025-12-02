using System.Text.Json;
using backend.Models;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class SpaceRepository : ISpaceRepository
{
    private readonly ApplicationDbContext _context;

    public SpaceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task InsertSpaceCacheAsync(string source, JsonDocument payload)
    {
        SpaceCacheEntity item = new SpaceCacheEntity
        {
            Source = source,
            Payload = payload.RootElement.GetRawText()
        };
        
        _context.SpaceCaches.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task<SpaceCacheEntity?> GetLastRecordBySource(string source)
    {
        return await _context.SpaceCaches
            .OrderByDescending(r => r.Id)
            .Where(s => s.Source == source)
            .LastOrDefaultAsync();
    }
}
