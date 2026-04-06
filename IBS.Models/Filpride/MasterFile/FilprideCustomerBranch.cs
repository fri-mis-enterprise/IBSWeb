using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models.Filpride.MasterFile
{
    public class FilprideCustomerBranch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public FilprideCustomer? Customer { get; set; }

        [StringLength(50)]
        public string BranchName { get; set; } = null!;

        [StringLength(200)]
        public string BranchAddress { get; set; } = null!;

        [StringLength(50)]
        public string BranchTin { get; set; } = null!;

        [NotMapped]
        public List<SelectListItem>? CustomerSelectList { get; set; }
    }
}
