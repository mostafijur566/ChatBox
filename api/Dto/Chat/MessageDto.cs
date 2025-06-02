using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dto.Chat
{
    public class MessageDto
    {
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";
        public string? MediaUrl { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsDeletedForEveryone { get; set; } = false;
    }
}