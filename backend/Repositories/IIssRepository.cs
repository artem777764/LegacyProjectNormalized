using System.Text.Json;
using backend.Models.Entities;

namespace backend.Repositories;

public interface IIssRepository
{
    Task InsertIssDataAsync(string sourceUrl, JsonDocument payload);
    Task<IssFetchLogEntity> GetLastRecord();
    Task<List<IssFetchLogEntity>> GetLastNRecords(int n);
}
