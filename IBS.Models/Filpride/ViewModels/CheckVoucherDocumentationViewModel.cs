using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.Filpride.ViewModels
{
    public class CheckVoucherDocumentationViewModel
    {
        public string? DocumentType { get; set; }

        [Display(Name = "Documented by another company?")]
        public bool? IsDocumentedByOtherCompany { get; set; }

        [Display(Name = "Documenting Company")]
        [StringLength(50)]
        public string? DocumentedByCompanyName { get; set; }

        public List<SelectListItem> Companies { get; set; } = [];
    }
}
