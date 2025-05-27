using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public bool IsGroup { get; set; } = false;
        public string? GroupName { get; set; }
        public string? GroupProfilePicture { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }
    }
}