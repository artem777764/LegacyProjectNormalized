using backend.PeriodicTasks;
using ClosedXML.Excel;

namespace backend.TelemetryService
{
    public class XlsxGenerator : IFileGeneratorXlsx
    {
        public async Task WriteXlsxAsync(string fullPath, string sourceFileName, TelemetryRecord record, CancellationToken token)
        {
            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Telemetry");

                ws.Cell(1, 1).Value = "recorded_at";
                ws.Cell(1, 2).Value = "logical";
                ws.Cell(1, 3).Value = "voltage";
                ws.Cell(1, 4).Value = "temp";
                ws.Cell(1, 5).Value = "source_file";
                ws.Cell(2, 1).Value = record.RecordedAt;
                ws.Cell(2, 1).Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                ws.Cell(2, 2).Value = record.LogicalRus;
                ws.Cell(2, 3).SetValue(record.Voltage);
                ws.Cell(2, 4).SetValue(record.Temp);
                ws.Cell(2, 5).Value = sourceFileName;
                ws.Columns(1, 5).AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                ms.Position = 0;
                var bytes = ms.ToArray();
                var tmpPath = fullPath + ".tmp";

                await File.WriteAllBytesAsync(tmpPath, bytes, token).ConfigureAwait(false);
                if (File.Exists(fullPath)) File.Delete(fullPath);
                File.Move(tmpPath, fullPath);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации XLSX: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
