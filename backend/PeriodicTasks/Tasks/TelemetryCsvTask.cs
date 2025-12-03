using System.Globalization;
using backend.TelemetryService;

namespace backend.PeriodicTasks.Tasks;

public class TelemetryCsvTask : IPeriodicTask
{
    public string Name => "telemetry_csv_gen";
    public TimeSpan Interval { get; }

    private readonly string _outDir;
    private readonly Random _random;
    private readonly ITelemetryFormatter _formatter;
    private readonly IFileGeneratorCsv _csvGen;
    private readonly IFileGeneratorXlsx _xlsxGen;

    public TelemetryCsvTask(
        IConfiguration cfg,
        ITelemetryFormatter formatter,
        IFileGeneratorCsv csvGen,
        IFileGeneratorXlsx xlsxGen)
    {
        _random = new Random();

        var intervalSec = cfg.GetValue<int?>("Telemetry:IntervalSec") ?? 300;
        Interval = TimeSpan.FromSeconds(intervalSec);

        _outDir = cfg.GetValue<string?>("Telemetry:OutDir") ?? "D:\\";
        if (!string.IsNullOrEmpty(_outDir) && !_outDir.EndsWith(Path.DirectorySeparatorChar))
            _outDir += Path.DirectorySeparatorChar;

        _formatter = formatter;
        _csvGen = csvGen;
        _xlsxGen = xlsxGen;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var record = CreateRecord();
            var ts = record.RecordedAt.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var csvName = $"telemetry_{ts}.csv";
            var xlsxName = $"telemetry_{ts}.xlsx";
            var csvPath = Path.Combine(_outDir, csvName);
            var xlsxPath = Path.Combine(_outDir, xlsxName);

            var dir = Path.GetDirectoryName(_outDir.TrimEnd(Path.DirectorySeparatorChar));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var t1 = _csvGen.WriteCsvAsync(csvPath, csvName, record, stoppingToken);
            var t2 = _xlsxGen.WriteXlsxAsync(xlsxPath, csvName, record, stoppingToken);

            await Task.WhenAll(t1, t2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в ExecuteAsync TelemetryCsvTask: {ex.Message}");
        }
    }

    private TelemetryRecord CreateRecord()
    {
        var now = DateTime.UtcNow;
        bool logical = _random.Next(0, 2) == 1;
        string logicalRus = logical ? "ИСТИНА" : "ЛОЖЬ";
        double voltage = RandDouble(3.2, 12.6);
        double temp = RandDouble(-50.0, 80.0);

        return new TelemetryRecord
        {
            RecordedAt = now,
            LogicalRus = logicalRus,
            Voltage = Math.Round(voltage, 2),
            Temp = Math.Round(temp, 2)
        };
    }

    private double RandDouble(double min, double max)
    {
        return min + _random.NextDouble() * (max - min);
    }
}