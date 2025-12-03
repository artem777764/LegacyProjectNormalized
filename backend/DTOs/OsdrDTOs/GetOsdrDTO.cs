namespace backend.DTOs.OsdrDTOs
{
    public class GetOsdrDTO
    {
        public required long Id { get; set; }
        public string? DatasetId { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public required DateTimeOffset FetchedAt { get; set; }
        public required DateTimeOffset? UpdatedAt { get; set; }
        public required string Payload { get; set; }
    }
}
