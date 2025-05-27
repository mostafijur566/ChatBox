using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dto.Account
{
    public class LoginDto
    {
        public string PhoneNumberOrUsername { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}