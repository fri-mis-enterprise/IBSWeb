using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.Models
{
    public class DepartmentAccessViewModel
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string[] Department { get; set; } = [];

        public string Module { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;
    }
}
