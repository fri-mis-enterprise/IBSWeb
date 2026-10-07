using IBS.DTOs;
using IBS.Models.Filpride.Books;

namespace IBSWeb.Areas.Filpride.Models
{
    public class GlImportViewModel
    {
        public GlImportPreview? Preview { get; set; }
        public string? PreviewToken { get; set; }
        public List<FilprideGlImportBatch> History { get; set; } = [];
    }
}
