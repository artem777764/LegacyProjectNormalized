using System.Text.Json;
using backend.Models;
using backend.Models.Entities;
using backend.Repositories;

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
}
