namespace IBS.DTOs
{
    public sealed class SalesInvoiceCollectionDetailsDto
    {
        public int InvoiceId { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal NetAmount { get; set; }

        public decimal VatAmount { get; set; }

        public decimal CwtBalance { get; set; }

        public decimal CwVatBalance { get; set; }

        public decimal Balance { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }
    }
}
