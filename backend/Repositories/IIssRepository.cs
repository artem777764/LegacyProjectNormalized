using System.Text.Json;

namespace backend.Repositories;

public interface IIssRepository
{
    Task InsertIssDataAsync(string sourceUrl, JsonDocument payload);
}
