namespace IBS.Models.Mobility.ViewModels
{
    public class FMSFuelSalesRawViewModel
    {
        public string stncode { get; set; } = null!;

        public string shiftrecid { get; set; } = null!;

        public int pumpnumber { get; set; }

        public string productcode { get; set; } = null!;

        public decimal opening { get; set; }

        public decimal closing { get; set; }

        public decimal price { get; set; }

        public DateOnly shiftdate { get; set; }

        public int shiftnumber { get; set; }

        public int pagenumber { get; set; }


    }
}
