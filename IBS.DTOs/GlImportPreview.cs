namespace IBS.DTOs
{
    public class GlImportPreview
    {
        public string FileName { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
        public List<GlImportRow> Rows { get; set; } = [];
        public List<string> Errors { get; set; } = [];
        public bool CanImport => Errors.Count == 0 && Rows.Count > 0;
        public decimal TotalDebit => Rows.Sum(row => row.Debit);
        public decimal TotalCredit => Rows.Sum(row => row.Credit);
        public int JournalCount => Rows.Select(row => (row.Date, row.Reference)).Distinct().Count();
    }
}
