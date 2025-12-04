using System.Globalization;
using backend.PeriodicTasks;

namespace backend.TelemetryService;

public class TelemetryFormatter : ITelemetryFormatter
{
    public string GetCsvHeader() =>
        "recorded_at,logical,voltage,temp,source_file";

    public string FormatRecordAsCsvLine(TelemetryRecord record, string sourceFileName)
    {
        string recordedAt = record.RecordedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string voltageTxt = record.Voltage.ToString("0.00", CultureInfo.InvariantCulture);
        string tempTxt = record.Temp.ToString("0.00", CultureInfo.InvariantCulture);

        return string.Join(",",
            recordedAt,
            QuoteCsv(record.LogicalRus),
            voltageTxt,
            tempTxt,
            QuoteCsv(sourceFileName)
        );
    }

    private static string QuoteCsv(string s)
    {
        if (s == null) return "\"\"";
        var inner = s.Replace("\"", "\"\"");
        return $"\"{inner}\"";
    }
}