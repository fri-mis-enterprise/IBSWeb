namespace IBS.Models.Mobility.ViewModels
{
    public class FMSCalibrationRawViewModel
    {
        public string stncode { get; set; } = null!;

        public string shiftrecid { get; set; } = null!;

        public int pumpnumber { get; set; }

        public string productcode { get; set; } = null!;

        public decimal quantity { get; set; }

        public decimal price { get; set; }

        public DateOnly shiftdate { get; set; }

        public int shiftnumber { get; set; }

        public int pagenumber { get; set; }
    }
}
