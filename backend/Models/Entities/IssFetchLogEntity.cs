using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities;

public class IssFetchLogEntity
{
    [Key]
    public long Id { get; set; }

    public DateTimeOffset FetchedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public string SourceUrl { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public string Payload { get; set; } = "{}";
}