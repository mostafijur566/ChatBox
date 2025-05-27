using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Phonenumber { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public string Prassword { get; set; } = string.Empty;
        public DateTime LastSeen { get; set; }
        public bool IsOnline { get; set; }
    }
}