using IBS.Models.Filpride.AccountsPayable;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IBS.Models.Filpride.ViewModels
{
    public class JournalVoucherViewModel
    {
        [Display(Name = "JV No")]
        public string? JVNo { get; set; }

        public int JVId { get; set; }

        public long SeriesNumber { get; set; }

        [Display(Name = "Transaction Date")]
        public DateOnly TransactionDate { get; set; }

        public string? References { get; set; }

        [Display(Name = "CV Id")]
        public int? CVId { get; set; }

        [Display(Name = "Employee")]
        [Required(ErrorMessage = "The Employee field is required.")]
        public int? EmployeeId { get; set; }

        [ForeignKey(nameof(CVId))]
        public FilprideCheckVoucherHeader? CheckVoucherHeader { get; set; }

        [NotMapped]
        public List<SelectListItem>? CheckVoucherHeaders { get; set; }

        [NotMapped]
        public List<SelectListItem>? Employees { get; set; }

        public string Particulars { get; set; } = null!;

        [Display(Name = "PR No")]
        public string? PRNo { get; set; }

        [NotMapped]
        public List<SelectListItem>? ProvisionalReceipts { get; set; }

        [Display(Name = "JV Reason")]
        public string JVReason { get; set; } = null!;

        public List<SelectListItem>? COA { get; set; }

        public List<SelectListItem>? SupplierList { get; set; }

        public string? Type { get; set; }

        public DateTime MinDate { get; set; }

        public List<JournalVoucherDetailViewModel>? Details { get; set; }
    }

    public class JournalVoucherDetailViewModel
    {
        public string AccountNumber { get; set; } = null!;
        public string AccountTitle { get; set; } = null!;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public int? SubAccountId { get; set; }
        public string? SubAccountCodeName { get; set; }
    }
}
