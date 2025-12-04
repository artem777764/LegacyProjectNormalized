using backend.Models;
using backend.Models.Entities;

namespace backend.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly ApplicationDbContext _context;

    public TelemetryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(TelemetryLogEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.TelemetryLogs.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}