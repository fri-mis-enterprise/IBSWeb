namespace IBS.Models.Filpride.ViewModels
{
    public class TerminatePlacementViewModel
    {
        public int PlacementId { get; set; }

        public decimal InterestDeposited { get; set; }

        public DateOnly InterestDepositedDate { get; set; }

        public string InterestDepositedTo { get; set; } = null!;

        public string InterestStatus { get; set; } = null!;

        public DateOnly TerminatedDate { get; set; }

        public string TerminationRemarks { get; set; } = null!;
    }

}
