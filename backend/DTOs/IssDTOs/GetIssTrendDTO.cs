namespace backend.DTOs.IssDTOs;

public class IssTrendDTO
    {
        public bool Movement { get; set; }
        public double DeltaKm { get; set; }
        public double DtSec { get; set; }
        public double? VelocityKmh { get; set; }
        public DateTimeOffset? FromTime { get; set; }
        public DateTimeOffset? ToTime { get; set; }
        public double? FromLat { get; set; }
        public double? FromLon { get; set; }
        public double? ToLat { get; set; }
        public double? ToLon { get; set; }
    }