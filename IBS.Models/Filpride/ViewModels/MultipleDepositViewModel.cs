namespace IBS.Models.Filpride.ViewModels
{
    public class MultipleDepositViewModel
    {
        public int BankId { get; set; }

        public DateOnly DepositedDate { get; set; }

        public DateOnly ClearedDate { get; set; }

        public int CollectionReceiptId { get; set; }

        public DateOnly TransactionDate { get; set; }
    }
}
