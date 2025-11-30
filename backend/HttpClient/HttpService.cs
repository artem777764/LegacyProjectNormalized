using System.Text.Json;

public class HttpService : IHttpService
{
    private readonly IHttpClientFactory _factory;

    public HttpService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    public async Task<string> GetStringAsync(string url, CancellationToken ct = default)
    {
        var client = CreateClient();
        using var res = await client.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync(ct);
    }

    public async Task<JsonDocument> GetJsonDocumentAsync(string url, CancellationToken ct = default)
    {
        var client = CreateClient();
        using var res = await client.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        var stream = await res.Content.ReadAsStreamAsync(ct);
        return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
    }
}
