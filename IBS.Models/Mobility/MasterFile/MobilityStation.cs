using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.Mobility.MasterFile
{
    public class MobilityStation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StationId { get; set; }

        [Display(Name = "POS Code")]
        public string PosCode { get; set; } = null!;

        [Key]
        [Display(Name = "Station Code")]
        [Column(TypeName = "varchar(5)")]
        public string StationCode { get; set; } = null!;

        [Display(Name = "Station Name")]
        [Column(TypeName = "varchar(50)")]
        public string StationName { get; set; } = null!;

        [Column(TypeName = "varchar(5)")]
        public string Initial { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "varchar(255)")]
        public string FolderPath { get; set; } = string.Empty;

        [Display(Name = "Created By")]
        [Column(TypeName = "varchar(50)")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedDate { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila"));

        [Display(Name = "Edited By")]
        [Column(TypeName = "varchar(50)")]
        public string? EditedBy { get; set; }

        [Display(Name = "Edited Date")]
        [Column(TypeName = "timestamp without time zone")]
        public DateTime EditedDate { get; set; }
    }
}
