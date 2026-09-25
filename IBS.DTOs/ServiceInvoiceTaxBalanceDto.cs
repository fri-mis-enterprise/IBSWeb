namespace IBS.DTOs
{
    public sealed class ServiceInvoiceTaxBalanceDto
    {
        public int ServiceInvoiceId { get; init; }

        public string InvoiceNo { get; init; } = string.Empty;

        public decimal CwtAmount { get; init; }

        public decimal CwtAmountPaid { get; init; }

        public decimal CwtBalance { get; init; }

        public decimal CwVatAmount { get; init; }

        public decimal CwVatAmountPaid { get; init; }

        public decimal CwVatBalance { get; init; }
    }
}
