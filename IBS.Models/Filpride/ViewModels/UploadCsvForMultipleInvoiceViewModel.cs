namespace IBS.Models.Filpride.ViewModels
{
    public class UploadCsvForMultipleInvoiceViewModel
    {
        public string CustomerName { get; set; } = null!;

        public string SalesInvoiceNo { get; set; } = null!;

        public DateOnly TransactionDate { get; set; }

        public string ReferenceNo { get; set; } = null!;

        public string Remarks { get; set; } = null!;

        public decimal CashAmount { get; set; }

        public DateOnly CheckDate { get; set; }

        public string CheckNo { get; set; } = null!;

        public string CheckBank { get; set; } = null!;

        public string CheckBranch { get; set; } = null!;

        public decimal CheckAmount { get; set; }

        public decimal ManagersCheckAmount { get; set; }

        public DateOnly ManagersCheckDate { get; set; }

        public string ManagersCheckNo { get; set; } = null!;

        public string ManagersCheckBank { get; set; } = null!;

        public string ManagersCheckBranch { get; set; } = null!;

        public decimal EWT { get; set; }

        public decimal WVAT { get; set; }

        public decimal Total { get; set; }

        public string BatchNumber { get; set; } = null!;

        public string Type { get; set; } = null!;

        public decimal SiAmount { get; set; }
    }
}
