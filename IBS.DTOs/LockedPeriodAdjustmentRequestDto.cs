using IBS.Models.Enums;

namespace IBS.DTOs
{
    public class LockedPeriodAdjustmentRequestDto
    {
        public Module Module { get; set; }

        public DateOnly TransactionDate { get; set; }

        public Module EntityType { get; set; }

        public string EntityNo { get; set; } = string.Empty;

        public LockedPeriodAdjustmentType AdjustmentType { get; set; }

        public decimal OldValue { get; set; }

        public decimal NewValue { get; set; }

        public decimal AdjustmentValue { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
    }
}
