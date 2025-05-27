using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dto.Account;
using api.Interfaces;
using api.Mappers;
using api.Models;
using api.Responses;
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
        private readonly IAccountRepository _accountRepo;
        public AccountController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            ITokenService tokenService,
            IAccountRepository accountRepo
            )
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _accountRepo = accountRepo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Phonenumber == dto.PhoneNumber))
            {
                return BadRequest(new ResponseWithStatuscode(400, "User already exists"));
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

            await _accountRepo.RegisterAsync(user);

            // Generate JWT token
            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                token = token,
                user = user.ToUserDto(),
                message = "User registered successfully"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _accountRepo.GetByPhoneNumberOrUsernameAsync(dto.PhoneNumberOrUsername);

            if (user == null)
            {
                return Unauthorized(new ResponseWithStatuscode(401, "Invalid credentials."));
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Prassword, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new ResponseWithStatuscode(401, "Invalid credentials."));

            // Generate JWT token
            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                token = token,
                user = user.ToUserDto()
            });
        }
    }
}