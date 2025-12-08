namespace backend.DTOs.SpaceDTOs;

public class GetSpaceItemSummaryDTO
{
    public DateTimeOffset? FetchedAt { get; set; }
    public string? Payload { get; set; }
}