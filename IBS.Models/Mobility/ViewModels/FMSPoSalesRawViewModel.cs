namespace IBS.Models.Mobility.ViewModels
{
    public class FMSPoSalesRawViewModel
    {
        public string stncode { get; set; } = null!;

        public string shiftrecid { get; set; } = null!;

        public string customercode { get; set; } = null!;

        public string tripticket { get; set; } = null!;

        public string drno { get; set; } = null!;

        public string driver { get; set; } = null!;

        public string plateno { get; set; } = null!;

        public string productcode { get; set; } = null!;

        public decimal quantity { get; set; }

        public decimal price { get; set; }

        public decimal contractprice { get; set; }

        public string time { get; set; } = null!;

        public DateOnly date { get; set; }

        public DateOnly shiftdate { get; set; }

        public int shiftnumber { get; set; }

        public int pagenumber { get; set; }
    }
}
