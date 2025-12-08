using backend.DTOs.SpaceDTOs;

public class SpaceSummaryDTO
{
    public Dictionary<string, GetSpaceItemSummaryDTO> Items { get; set; } = new();
    public int osdrCount { get; set; }
}
