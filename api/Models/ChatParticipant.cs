using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ChatParticipant
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; }

        // Navigation proparties
        [ForeignKey("ChatId")]
        public Chat? Chat { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}