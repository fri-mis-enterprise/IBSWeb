namespace IBS.DTOs
{
    public class LogMessageDto
    {
        public string StationName { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string CsvStatus { get; set; } = null!;

        public string OpeningFileStatus { get; set; } = null!;

        public string HowManyImported { get; set; } = null!;

        public string Error { get; set; } = null!;
    }
}
