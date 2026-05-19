using System.ComponentModel.DataAnnotations;

namespace IBSWeb.Areas.Admin.ViewModels
{
    public class DbSyncViewModel
    {
        [Required]
        public string Server { get; set; } = string.Empty;

        [Required]
        public int Port { get; set; } = 5432;

        [Required]
        public string Database { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
