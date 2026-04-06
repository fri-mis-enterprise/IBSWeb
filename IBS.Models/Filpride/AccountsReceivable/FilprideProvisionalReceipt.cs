using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.Filpride.MasterFile;

namespace IBS.Models.Filpride.AccountsReceivable
{
    public class FilprideProvisionalReceipt : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(20)]
        public string SeriesNumber { get; set; } = string.Empty;

        public DateOnly TransactionDate { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public FilprideEmployee Employee { get; set; } = null!;

        public string ReferenceNo { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;

        [Column(TypeName = "numeric(18,4)")]
        public decimal CashAmount { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal CheckAmount { get; set; }

        public DateOnly? CheckDate { get; set; }

        public string? CheckNo { get; set; }

        public string? CheckBank { get; set; }

        public string? CheckBranch { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal ManagersCheckAmount { get; set; }

        public DateOnly? ManagersCheckDate { get; set; }

        public string? ManagersCheckNo { get; set; }

        public string? ManagersCheckBank { get; set; }

        public string? ManagersCheckBranch { get; set; }

        public int? BankId { get; set; }

        public FilprideBankAccount? BankAccount { get; set; }

        public string? BankAccountNo { get; set; }

        public string? BankAccountName { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal EWT { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal WVAT { get; set; }

        [Column(TypeName = "numeric(18,4)")]
        public decimal Total { get; set; }

        [StringLength(20)]
        public string Company { get; set; } = string.Empty;

        public bool IsPrinted { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [StringLength(20)]
        public string Type { get; set; } = string.Empty;

        public DateOnly? DepositedDate { get; set; }

        public DateOnly? ClearedDate { get; set; }

        public string? BatchNumber { get; set; }
    }
}
