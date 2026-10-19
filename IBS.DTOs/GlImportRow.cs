using IBS.Models.Enums;

namespace IBS.DTOs
{
    public class GlImportRow
    {
        public int RowNumber { get; set; }
        public DateOnly Date { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public CounterpartyType? CounterpartyType { get; set; }
        public int? CounterpartyId { get; set; }
        public string? CounterpartyName { get; set; }
        public string Company { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public int AccountId { get; set; }
        public SubAccountType? SubAccountType { get; set; }
        public int? SubAccountId { get; set; }
        public string? SubAccountName { get; set; }
    }
}
