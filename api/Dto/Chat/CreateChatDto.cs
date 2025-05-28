using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dto.Chat
{
    public class CreateChatDto
    {
        [Required]
        public bool IsGroup { get; set; } = false;
        public string? GroupName { get; set; }
        public string? GroupProfilePicture { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        // For 1-to-1 chat: should contain exactly 2 unique user IDs
        public List<int> ParticipantUserIds { get; set; } = new();
    }
}