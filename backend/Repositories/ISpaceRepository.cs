using System.Text.Json;

namespace backend.Repositories;

public interface ISpaceRepository
{
    Task InsertSpaceCacheAsync(string source, JsonDocument payload);
}
