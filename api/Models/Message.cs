using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } = "text";
        public string? MediaType { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsDeletedForEveryone { get; set; } = false;

        // Navigation properties
        [ForeignKey("ChatId")]
        public virtual Chat Chat { get; set; }

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
        public virtual ICollection<MessageStatus> Statuses { get; set; }
    }
}