namespace IBS.DTOs
{
    public class StationDto
    {
        public int StationId { get; set; }

        public string StationCode { get; set; } = null!;

        public string StationName { get; set; } = null!;

        public string StationPOSCode { get; set; } = null!;
    }
}
