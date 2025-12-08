using System.Text.Json;

public interface IHttpService
{
    HttpClient CreateClient();
    Task<string> GetStringAsync(string url, CancellationToken ct = default);
    Task<JsonDocument> GetJsonDocumentAsync(string url, CancellationToken ct = default);
}
