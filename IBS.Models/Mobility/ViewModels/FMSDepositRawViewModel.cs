namespace IBS.Models.Mobility.ViewModels
{
    public class FMSDepositRawViewModel
    {
        public string stncode { get; set; } = null!;

        public string shiftrecid { get; set; } = null!;

        public DateOnly date { get; set; }

        public string accountno { get; set; } = null!;

        public decimal amount { get; set; }

        public DateOnly shiftdate { get; set; }

        public int shiftnumber { get; set; }

        public int pagenumber { get; set; }
    }
}
