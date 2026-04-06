using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.Mobility
{
    public class MobilityFMSPoSales
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string StationCode { get; set; } = null!;

        public string ShiftRecordId { get; set; } = null!;

        public string ProductCode { get; set; } = null!;

        public string CustomerCode { get; set; } = null!;

        public string TripTicket { get; set; } = null!;

        public string DrNumber { get; set; } = null!;

        public string Driver { get; set; } = null!;

        public string PlateNo { get; set; } = null!;

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal ContractPrice { get; set; }

        public TimeOnly Time { get; set; }

        public DateOnly Date { get; set; }

        public DateOnly ShiftDate { get; set; }

        public int ShiftNumber { get; set; }

        public int PageNumber { get; set; }

        public bool IsProcessed { get; set; }
    }
}
