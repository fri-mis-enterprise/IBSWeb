namespace IBS.Models.Filpride.ViewModels
{
    public class GoogleDriveFileViewModel
    {
        public string FileName { get; set; } = null!;
        public string FileLink { get; set; } = null!;
        public byte[]? FileContent { get; set; }
        public bool DoesExist { get; set; }
        public string? FileId { get; set; }
    }
}
