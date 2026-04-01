using System.ComponentModel.DataAnnotations;
using IBS.Models.Enums;

namespace IBS.Models.Filpride.ViewModels
{
    public class PRCreateViewModel : ProvisionalReceiptViewModel
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string Type { get; set; } = nameof(DocumentType.Documented);
    }
}
