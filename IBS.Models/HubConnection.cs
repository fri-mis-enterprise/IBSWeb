using System.ComponentModel.DataAnnotations;

namespace IBS.Models
{
    public class HubConnection
    {
        [Key]
        public Guid Id { get; set; }

        public string ConnectionId { get; set; } = null!;

        public string UserName { get; set; } = null!;
    }
}
