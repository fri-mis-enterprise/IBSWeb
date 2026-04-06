using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.Mobility
{
    public class MobilityFuelDelivery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid FuelDeliveryId { get; set; }

        public int pagenumber { get; set; }

        [Column(TypeName = "varchar(5)")]
        public string stncode { get; set; } = null!;

        public string cashiercode { get; set; } = null!; //remove the "E" when saving in actual database

        public int shiftnumber { get; set; }

        [Column(TypeName = "date")]
        public DateOnly shiftdate { get; set; }

        [Column(TypeName = "time without time zone")]
        public TimeOnly timein { get; set; }

        [Column(TypeName = "time without time zone")]
        public TimeOnly timeout { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string driver { get; set; } = null!;

        [Column(TypeName = "varchar(100)")]
        public string hauler { get; set; } = null!;

        [Column(TypeName = "varchar(50)")]
        public string platenumber { get; set; } = null!;

        [Column(TypeName = "varchar(50)")]
        public string drnumber { get; set; } = null!; //it should be int in actual database so remove the "DR"

        [Column(TypeName = "varchar(50)")]
        public string wcnumber { get; set; } = null!;

        public int tanknumber { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string productcode { get; set; } = null!;

        [Column(TypeName = "numeric(18,4)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal quantity { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal purchaseprice { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal sellprice { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal volumebefore { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal volumeafter { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string receivedby { get; set; } = null!;

        [Column(TypeName = "varchar(50)")]
        public string createdby { get; set; } = null!; //remove the "E" when saving in actual database

        [Column(TypeName = "timestamp without time zone")]
        public DateTime createddate { get; set; }
    }
}
