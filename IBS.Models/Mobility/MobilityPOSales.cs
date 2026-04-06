using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.Mobility
{
    public class MobilityPOSales : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int POSalesId { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string POSalesNo { get; set; } = null!;

        [Column(TypeName = "varchar(20)")]
        public string ShiftRecId { get; set; } = null!;

        [Column(TypeName = "varchar(5)")]
        public string StationCode { get; set; } = null!;

        [Column(TypeName = "varchar(5)")]
        public string CashierCode { get; set; } = null!;

        public int ShiftNo { get; set; }

        [Column(TypeName = "date")]
        public DateOnly POSalesDate { get; set; }

        [Column(TypeName = "time without time zone")]
        public TimeOnly? POSalesTime { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CustomerCode { get; set; } = null!;

        [Column(TypeName = "varchar(50)")]
        public string Driver { get; set; } = null!;

        [Column(TypeName = "varchar(50)")]
        public string PlateNo { get; set; } = null!;

        [Column(TypeName = "varchar(50)")]
        public string DrNo { get; set; } = null!;

        [Column(TypeName = "varchar(20)")]
        public string TripTicket { get; set; } = null!;

        [Column(TypeName = "varchar(10)")]
        public string ProductCode { get; set; } = null!;

        [Column(TypeName = "numeric(18,4)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal Quantity { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000;(#,##0.0000)}", ApplyFormatInEditMode = true)]
        public decimal ContractPrice { get; set; }
    }
}
