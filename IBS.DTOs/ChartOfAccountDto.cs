namespace IBS.DTOs
{
    public class ChartOfAccountDto
    {
        public string AccountNumber { get; set; } = null!;

        public string AccountName { get; set; } = null!;

        public string AccountType { get; set; } = null!;

        public string? Parent { get; set; }

        public int Level { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal Balance { get; set; }

        public List<ChartOfAccountDto>? Children { get; set; }
    }
}
