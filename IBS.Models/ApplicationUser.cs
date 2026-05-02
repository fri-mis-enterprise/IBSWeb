using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace IBS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = null!;

        public string Department { get; set; } = null!;

        public ICollection<UserNotification> UserNotifications { get; set; } = null!;

        public string? Position { get; set; }

        [Required]
        [Column(TypeName = "boolean")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime CreatedDate { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila"));

        [Column(TypeName = "timestamp")]
        public DateTime? ModifiedDate { get; set; }

        [MaxLength(256)]
        public string? ModifiedBy { get; set; }
    }
}
