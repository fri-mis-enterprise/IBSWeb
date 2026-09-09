using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.Enums;

namespace IBS.Models.Filpride.MasterFile
{
    public class FilprideMasterFileRequest
    {
        public int Id { get; set; }

        public FilprideMasterFileType MasterFileType { get; set; }

        [Column(TypeName = "jsonb")]
        public string PayloadJson { get; set; } = null!;

        [ConcurrencyCheck]
        public FilprideMasterFileRequestStatus Status { get; set; } = FilprideMasterFileRequestStatus.ForApproval;

        [StringLength(450)]
        public string RequestedBy { get; set; } = null!;

        [StringLength(200)]
        public string RequestedByName { get; set; } = null!;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime RequestedDate { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime LastModifiedDate { get; set; }

        [StringLength(200)]
        public string? ApprovedBy { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ApprovedDate { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }
    }
}
