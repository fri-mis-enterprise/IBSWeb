using System.ComponentModel.DataAnnotations;

namespace IBS.Models.Filpride.ViewModels
{
    public class DispatchReportViewModel
    {
        public string ReportType { get; set; } = null!;

        public DateOnly DateFrom { get; set; }

        public DateOnly DateTo { get; set; }

        [Display(Name = "Status Filter")]
        public string DRStatusFilter { get; set; } = "ValidOnly";
    }
}
