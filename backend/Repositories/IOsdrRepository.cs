using System.Text.Json;
using backend.Models.Entities;

namespace backend.Repositories;

public interface IOsdrRepository
{
    Task<int> SaveOsdrItemsAsync(JsonDocument doc);
    Task<List<OsdrItemEntity>> GetLastNRecords(int n);
    Task<int> GetDatasetsCount();
}
