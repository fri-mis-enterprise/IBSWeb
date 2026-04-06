using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IBS.Models.Filpride.AccountsPayable;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.Mobility.ViewModels
{
    public class JournalVoucherViewModel
    {
        [Display(Name = "JV No")]
        public string? JournalVoucherHeaderNo { get; set; }

        public int JournalVoucherHeaderId { get; set; }

        [Display(Name = "Transaction Date")]
        public DateOnly TransactionDate { get; set; }

        public string? References { get; set; }

        [Display(Name = "CV Id")]
        public int? CheckVoucherHeaderId { get; set; }

        [NotMapped]
        public List<SelectListItem>? CheckVoucherHeaders { get; set; }

        public string Particulars { get; set; } = null!;

        [Display(Name = "CR No")]
        public string? CRNo { get; set; }

        [Display(Name = "JV Reason")]
        public string JVReason { get; set; } = null!;

        [NotMapped]
        public List<SelectListItem>? COA { get; set; }

        [Required]
        public string[] AccountNumber { get; set; } = null!;

        [Required]
        public string[] AccountTitle { get; set; } = null!;

        [Required]
        public decimal[] Debit { get; set; } = null!;

        [Required]
        public decimal[] Credit { get; set; } = null!;

        public string? Type { get; set; }

        public string StationCode { get; set; } = string.Empty;
    }
}
