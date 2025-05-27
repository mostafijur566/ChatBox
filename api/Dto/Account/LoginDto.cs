using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dto.Account
{
    public class LoginDto
    {
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
}