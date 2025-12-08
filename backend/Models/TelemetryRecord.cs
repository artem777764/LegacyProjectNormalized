namespace backend.PeriodicTasks;

public class TelemetryRecord
{
    public DateTime RecordedAt { get; set; }
    public string LogicalRus { get; set; } = "";
    public double Voltage { get; set; }
    public double Temp { get; set; }
}
