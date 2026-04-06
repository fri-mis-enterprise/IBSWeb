using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.Mobility
{
    public class MobilityLogReport
    {
        [Key]
        public Guid Id { get; set; }

        public string Reference { get; set; } = null!;

        public int ReferenceId { get; set; }

        public string Module { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? OriginalValue { get; set; }

        public string? AdjustedValue { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime TimeStamp { get; set; }

        public string ModifiedBy { get; set; } = null!;
    }
}
