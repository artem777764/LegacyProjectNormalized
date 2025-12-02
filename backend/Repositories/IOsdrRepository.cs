using System.Text.Json;

namespace backend.Repositories;

public interface IOsdrRepository
{
    Task<int> SaveOsdrItemsAsync(JsonDocument doc);
}
