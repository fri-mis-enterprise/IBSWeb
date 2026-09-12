using System.ComponentModel.DataAnnotations;
using IBS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.Filpride.ViewModels
{
    public class CollectionCategoryViewModel
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Select a credit account.")]
        [Display(Name = "Credit account")]
        public int CreditAccountId { get; set; }
        public List<SelectListItem> CreditAccounts { get; set; } = [];
        [EnumDataType(typeof(CollectionTaggingRequirement))]
        [Display(Name = "Tagging requirement")]
        public CollectionTaggingRequirement TaggingRequirement { get; set; }
        [Display(Name = "Company")]
        public bool AllowCompany { get; set; }
        [Display(Name = "Employee")]
        public bool AllowEmployee { get; set; }
        [Display(Name = "Bank Account")]
        public bool AllowBankAccount { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        public bool IsUsed { get; set; }
    }
}
