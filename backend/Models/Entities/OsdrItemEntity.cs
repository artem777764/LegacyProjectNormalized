using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities;

public class OsdrItemEntity
{
    [Key]
    public long Id { get; set; }

    public string? DatasetId { get; set; }

    public string? Title { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset InsertedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "jsonb")]
    public string Raw { get; set; } = "{}";
}