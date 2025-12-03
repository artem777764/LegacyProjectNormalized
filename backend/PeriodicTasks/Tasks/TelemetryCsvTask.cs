using System.Globalization;
using System.Text;

namespace backend.PeriodicTasks
{
    public class TelemetryCsvTask : IPeriodicTask
    {
        public string Name => "telemetry_csv_gen";
        public TimeSpan Interval { get; }

        private readonly string _outDir;
        private readonly Random _random;

        public TelemetryCsvTask(IConfiguration cfg, ILogger<TelemetryCsvTask> logger)
        {
            _random = new Random();

            var intervalSec = cfg.GetValue<int?>("Telemetry:IntervalSec") ?? 300;
            Interval = TimeSpan.FromSeconds(intervalSec);

            _outDir = cfg.GetValue<string?>("Telemetry:OutDir") ?? "D:\\";
            if (!string.IsNullOrEmpty(_outDir) && !_outDir.EndsWith(Path.DirectorySeparatorChar))
                _outDir += Path.DirectorySeparatorChar;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (stoppingToken.IsCancellationRequested) return;
                await GenerateCsvAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ExecuteAsync TelemetryCsvTask: {ex.Message}");
            }
        }
        private async Task GenerateCsvAsync(CancellationToken token)
        {
            try
            {
                var dir = Path.GetDirectoryName(_outDir.TrimEnd(Path.DirectorySeparatorChar));
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var now = DateTime.UtcNow;
                var ts = now.ToString("yyyyMMdd_HHmmss");
                var filename = $"telemetry_{ts}.csv";
                var fullpath = Path.Combine(_outDir, filename);

                bool logical = _random.Next(0, 2) == 1;
                string logicalRus = logical ? "ИСТИНА" : "ЛОЖЬ";
                double voltage = RandDouble(3.2, 12.6);
                double temp = RandDouble(-50.0, 80.0);

                var sb = new StringBuilder();
                sb.AppendLine("recorded_at,logical,voltage,temp,source_file");

                string recordedAt = now.ToString("yyyy-MM-dd HH:mm:ss");
                string voltageTxt = voltage.ToString("0.00", CultureInfo.InvariantCulture);
                string tempTxt = temp.ToString("0.00", CultureInfo.InvariantCulture);

                sb.AppendLine(string.Join(",",
                    recordedAt,
                    QuoteCsv(logicalRus),
                    voltageTxt,
                    tempTxt,
                    QuoteCsv(filename)
                ));

                var tmpPath = fullpath + ".tmp";
                await File.WriteAllTextAsync(tmpPath, sb.ToString(), Encoding.UTF8, token);
                if (File.Exists(fullpath))
                    File.Delete(fullpath);
                File.Move(tmpPath, fullpath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации CSV: {ex}");
            }
        }

        private static string QuoteCsv(string s)
        {
            if (s == null) return "\"\"";
            var inner = s.Replace("\"", "\"\"");
            return $"\"{inner}\"";
        }

        private double RandDouble(double min, double max)
        {
            return min + _random.NextDouble() * (max - min);
        }
    }
}