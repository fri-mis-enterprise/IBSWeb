using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.Filpride.ViewModels
{
    public abstract class ProvisionalReceiptViewModel
    {
        [Required]
        [Display(Name = "Transaction Date")]
        public DateOnly TransactionDate { get; set; }

        [Required(ErrorMessage = "The employee is required.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        public List<SelectListItem>? Employees { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Reference No.")]
        public string ReferenceNo { get; set; } = string.Empty;

        [StringLength(255)]
        public string Remarks { get; set; } = string.Empty;

        [Display(Name = "Cash Amount")]
        public decimal CashAmount { get; set; }

        [Display(Name = "Check Amount")]
        public decimal CheckAmount { get; set; }

        [Display(Name = "Check Date")]
        public DateOnly? CheckDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Check No.")]
        public string? CheckNo { get; set; }

        [StringLength(100)]
        [Display(Name = "Check Bank")]
        public string? CheckBank { get; set; }

        [StringLength(100)]
        [Display(Name = "Check Branch")]
        public string? CheckBranch { get; set; }

        [Display(Name = "Manager's Check Amount")]
        public decimal ManagersCheckAmount { get; set; }

        [Display(Name = "Manager's Check Date")]
        public DateOnly? ManagersCheckDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Manager's Check No.")]
        public string? ManagersCheckNo { get; set; }

        [StringLength(100)]
        [Display(Name = "Manager's Check Bank")]
        public string? ManagersCheckBank { get; set; }

        [StringLength(100)]
        [Display(Name = "Manager's Check Branch")]
        public string? ManagersCheckBranch { get; set; }

        public decimal EWT { get; set; }

        public decimal WVAT { get; set; }

        public decimal Total { get; set; }

        public DateTime MinDate { get; set; }

        [Display(Name = "Batch#")]
        public string? BatchNumber { get; set; }
    }
}
