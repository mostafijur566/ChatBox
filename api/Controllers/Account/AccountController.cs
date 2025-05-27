using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dto.Account;
using api.Interfaces;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers.Account
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;
        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, ITokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> register(RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Phonenumber == dto.PhoneNumber))
            {
                return BadRequest("User already exists");
            }

            var user = new User
            {
                Username = dto.Username,
                Phonenumber = dto.PhoneNumber,
                Prassword = "",
                LastSeen = DateTime.UtcNow,
                IsOnline = true
            };

            user.Prassword = _passwordHasher.HashPassword(user, dto.password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }        

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Phonenumber == dto.PhoneNumber);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Prassword, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials.");

            // Generate JWT token
            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                token = token,
                userId = user.Id,
                username = user.Username
            });
        }
    }
}