using System.ComponentModel.DataAnnotations;

namespace IBS.Models.Filpride.ViewModels
{
    public class TransactionMasterControlViewModel
    {
        [Required]
        [Display(Name = "Reference No")]
        public string ReferenceNo { get; set; } = null!;

        public string? TransactionType { get; set; } // CV or JV

        [Required]
        [Display(Name = "Transaction Date")]
        public DateOnly Date { get; set; }

        [Required]
        [StringLength(1000)]
        public string Particulars { get; set; } = null!;

        [Display(Name = "Payment For")]
        public string? PaymentFor { get; set; }

        // CV specific fields
        [StringLength(150)]
        public string? Payee { get; set; }

        [StringLength(50)]
        [Display(Name = "Check No")]
        public string? CheckNo { get; set; }

        [Display(Name = "Check Date")]
        public DateOnly? CheckDate { get; set; }
        
        public bool IsFound { get; set; }
    }
}
