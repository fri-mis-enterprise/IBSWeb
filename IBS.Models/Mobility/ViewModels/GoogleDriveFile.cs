namespace IBS.Models.Mobility.ViewModels
{
    public class GoogleDriveFileViewModel
    {
        public string FileName { get; set; } = null!;

        public string FileLink { get; set; } = null!;

        public byte[] FileContent { get; set; } = null!;
    }
}
