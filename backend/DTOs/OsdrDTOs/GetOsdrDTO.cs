namespace backend.DTOs.OsdrDTOs
{
    public class GetOsdrDTO
    {
        public required long Id { get; set; }
        public required DateTimeOffset FetchedAt { get; set; }
        public required string Source { get; set; }
        public required string Payload { get; set; }
    }
}
