using System.Text.Json;
using System.Threading.Tasks;

namespace backend.PeriodicTasks;

public class ApiSender
{
    private readonly IHttpService _httpClient;

    public ApiSender(IHttpService httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JsonDocument?> Send(string url, CancellationToken stoppingToken = default)
    {
        try
        {
            Console.WriteLine($"fetching: {url}");
            var doc = await _httpClient.GetJsonDocumentAsync(url, stoppingToken);
            return doc;
        }
        catch (HttpRequestException hre)
        {
            Console.WriteLine($"{url}: HTTP error: {hre.Message}");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"{url}: cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{url}: unexpected error: {ex}");
        }
        return null;
    }
}