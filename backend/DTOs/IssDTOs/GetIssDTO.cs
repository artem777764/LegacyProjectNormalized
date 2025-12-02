namespace backend.DTOs.IssDTOs;

public class GetIssDTO
{
    public required long Id { get; set; }
    public required DateTimeOffset Fetched_at { get; set; }
    public required string Source_url { get; set; }
    public required string Payload { get; set; }
}