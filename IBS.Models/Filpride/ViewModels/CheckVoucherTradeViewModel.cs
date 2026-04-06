using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IBS.Models.Filpride.ViewModels
{
    public class CheckVoucherTradeViewModel
    {
        public int CVId { get; set; }

        public string? CVNo { get; set; }

        [StringLength(100)]
        public string? OldCVNo { get; set; }

        public List<SelectListItem>? Suppliers { get; set; }

        [Required]
        [StringLength(150)]
        public string Payee { get; set; } = null!;

        [Required]
        [Display(Name = "Supplier Address")]
        public string SupplierAddress { get; set; } = null!;

        [Required]
        [Display(Name = "Supplier Tin Number")]
        public string SupplierTinNo { get; set; } = null!;

        [Required]
        [Display(Name = "Supplier No")]
        public int SupplierId { get; set; }

        public List<SelectListItem>? PONo { get; set; }

        [Display(Name = "PO No.")]
        public string[]? POSeries { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        public DateOnly TransactionDate { get; set; }

        public List<SelectListItem>? BankAccounts { get; set; }

        [Required]
        [Display(Name = "Bank Accounts")]
        public int? BankId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Check #")]
        [RegularExpression(@"^(?:\d{7,}|)$", ErrorMessage = "Invalid format. Please enter CV number minimum 7 digits.")]
        public string CheckNo { get; set; } = null!;

        [Required]
        [Display(Name = "Check Date")]
        public DateOnly CheckDate { get; set; }

        [StringLength(1000)]
        public string Particulars { get; set; } = null!;

        public List<SelectListItem>? COA { get; set; }

        [Required]
        public string[] AccountNumber { get; set; } = null!;

        [Required]
        public string[] AccountTitle { get; set; } = null!;

        [Required]
        public decimal[] Debit { get; set; } = null!;

        [Required]
        public decimal[] Credit { get; set; } = null!;

        public List<CheckVoucherTradeAccountingEntryViewModel> AdditionalAccountingEntries { get; set; } = [];

        public decimal DefaultPayableAmount { get; set; }

        public decimal CashInBankAmount { get; set; }

        //others
        public string? CreatedBy { get; set; }

        public List<ReceivingReportList> RRs { get; set; } = null!;

        public string? AdvancesCVNo { get; set; }

        [Display(Name = "Applied Advance Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Applied advance amount cannot be negative.")]
        public decimal AppliedAdvanceAmount { get; set; }

        public string? Type { get; set; }

        public DateTime MinDate { get; set; }
    }

    public class CheckVoucherTradeAccountingEntryViewModel
    {
        public string AccountNumber { get; set; } = string.Empty;

        public string AccountTitle { get; set; } = string.Empty;

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }
    }

    public class ReceivingReportList
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }
    }
}
