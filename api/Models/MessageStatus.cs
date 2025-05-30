using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class MessageStatus
    {
        public int Id { get; set; }

        public int MessageId { get; set; }

        public int UserId { get; set; }

        public string Status { get; set; } // "sent", "delivered", "read"

        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("MessageId")]
        public virtual Message Message { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}