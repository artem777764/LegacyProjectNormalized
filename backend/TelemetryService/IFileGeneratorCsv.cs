using backend.PeriodicTasks;

namespace backend.TelemetryService;

public interface IFileGeneratorCsv
{
    Task WriteCsvAsync(string fullPath, string sourceFileName, TelemetryRecord record, CancellationToken token);
}