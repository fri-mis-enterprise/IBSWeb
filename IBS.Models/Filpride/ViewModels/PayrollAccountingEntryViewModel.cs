namespace IBS.Models.Filpride.ViewModels
{
    public class PayrollAccountingEntryViewModel
    {
        public string AccountNumber { get; set; } = null!;
        public string AccountTitle { get; set; } = null!;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public int? MultipleSupplierId { get; set; }
        public string? MultipleSupplierCodeName { get; set; }
    }
}
