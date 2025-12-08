using System.Text;
using backend.PeriodicTasks;

namespace backend.TelemetryService;

public class CsvGenerator : IFileGeneratorCsv
{
    private readonly ITelemetryFormatter _formatter;

    public CsvGenerator(ITelemetryFormatter formatter)
    {
        _formatter = formatter;
    }

    public async Task WriteCsvAsync(string fullPath, string sourceFileName, TelemetryRecord record, CancellationToken token)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine(_formatter.GetCsvHeader());
            sb.AppendLine(_formatter.FormatRecordAsCsvLine(record, sourceFileName));

            var tmpPath = fullPath + ".tmp";
            await File.WriteAllTextAsync(tmpPath, sb.ToString(), Encoding.UTF8, token);

            if (File.Exists(fullPath)) File.Delete(fullPath);
            File.Move(tmpPath, fullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при генерации CSV: {ex.Message}");
        }
    }
}
