using backend.PeriodicTasks;

namespace backend.TelemetryService;

public interface IFileGeneratorXlsx
{
    Task WriteXlsxAsync(string fullPath, string sourceFileName, TelemetryRecord record, CancellationToken token);
}