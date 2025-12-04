using backend.PeriodicTasks;

namespace backend.TelemetryService;

public interface ITelemetryFormatter
{
    string GetCsvHeader();
    string FormatRecordAsCsvLine(TelemetryRecord record, string sourceFileName);
}