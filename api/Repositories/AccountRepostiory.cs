using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class AccountRepostiory : IAccountRepository
    {
        private readonly ApplicationDbContext _context;
        public AccountRepostiory(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByPhoneNumberOrUsernameAsync(string phoneNumberOrUsername)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Phonenumber == phoneNumberOrUsername || u.Username == phoneNumberOrUsername);
        }

        public async Task<User> RegisterAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }
    
    }
}