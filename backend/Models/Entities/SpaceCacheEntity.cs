using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities;

public class SpaceCacheEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string Source { get; set; } = null!;

    public DateTimeOffset FetchedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "jsonb")]
    public string Payload { get; set; } = "{}";
}