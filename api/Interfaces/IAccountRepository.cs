using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IAccountRepository
    {
        public Task<User> RegisterAsync(User user);
        public Task<User?> GetByPhoneNumberOrUsernameAsync(string phoneNumberOrUsername);
    }
}