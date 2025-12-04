using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities;

public class TelemetryLogEntity
{
    [Key]
    public long Id { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public bool Logical { get; set; }

    [Required]
    public string LogicalRus { get; set; } = null!;

    public double Voltage { get; set; }
    public double Temp { get; set; }

    [Required]
    public string SourceFile { get; set; } = null!;
}
