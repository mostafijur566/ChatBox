using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dto.Account
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(20, ErrorMessage = "Username cannt be over 20 characters")]
        public string Username { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password cannot be less than 6 characters")]
        public string password { get; set; }
    }
}