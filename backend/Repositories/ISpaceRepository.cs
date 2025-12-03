using System.Text.Json;
using backend.Models.Entities;

namespace backend.Repositories;

public interface ISpaceRepository
{
    Task InsertSpaceCacheAsync(string source, JsonDocument payload);
    Task<SpaceCacheEntity?> GetLastRecordBySource(string source);
}
