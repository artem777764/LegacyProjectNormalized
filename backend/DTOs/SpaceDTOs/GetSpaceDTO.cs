namespace backend.DTOs.SpaceDTOs;

public class GetSpaceDTO
{
    public required string Source { get; set; }
    public required DateTimeOffset Fetched_at { get; set; }
    public required string Payload { get; set; }
}