namespace IBS.DTOs
{
    public class PublicHolidayDto
    {
        public DateTime Date { get; set; }
        public string LocalName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public bool Fixed { get; set; }
        public bool Global { get; set; }
        public string[] Counties { get; set; } = null!;
        public int? LaunchYear { get; set; }
        public string[] Types { get; set; } = null!;
    }
}
