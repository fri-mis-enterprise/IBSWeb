using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.Filpride.Books
{
    public class FilprideGlImportBatch
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(255)]
        public string FileName { get; set; } = null!;

        [StringLength(64)]
        public string FileHash { get; set; } = null!;

        [StringLength(100)]
        public string ImportedBy { get; set; } = null!;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime ImportedAt { get; set; }

        public int RowCount { get; set; }
        public int JournalCount { get; set; }

        [Column(TypeName = "date")]
        public DateOnly DateFrom { get; set; }

        [Column(TypeName = "date")]
        public DateOnly DateTo { get; set; }

        [Column(TypeName = "numeric(28,4)")]
        public decimal TotalDebit { get; set; }

        [Column(TypeName = "numeric(28,4)")]
        public decimal TotalCredit { get; set; }
    }
}
