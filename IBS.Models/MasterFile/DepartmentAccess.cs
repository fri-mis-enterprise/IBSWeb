using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.MasterFile
{
    public class DepartmentAccess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string[] Department { get; set; } = [];

        public string Module { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;


        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedDate { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila"));

        public string EditedBy { get; set; } = string.Empty;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime EditedDate { get; set; } = DateTime.MinValue;

    }
}
