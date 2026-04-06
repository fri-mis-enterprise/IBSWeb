using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.Mobility
{
    public class MobilityStationPump
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PumpId { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string StationCode { get; set; } = null!;

        [Column(TypeName = "varchar(15)")]
        public string ProductCode { get; set; } = null!;

        public int PosPump { get; set; }

        public int FmsPump { get; set; }
    }
}
