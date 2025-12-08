using backend.Models.Entities;

namespace backend.Repositories;

public interface ITelemetryRepository
{
    Task InsertAsync(TelemetryLogEntity entity, CancellationToken cancellationToken = default);
}