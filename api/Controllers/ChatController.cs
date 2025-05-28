using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.Dto.Chat;
using api.Models;
using api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers.Account
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChatById(int chatId)
        {
            var chat = await _context.Chats.FindAsync(chatId);
            if (chat == null)
                return NotFound();

            return Ok(chat);
        }

        [HttpPost]
        [Authorize] // Required to get claims from JWT
        public async Task<IActionResult> CreateChat([FromForm] CreateChatDto dto)
        {
            // Extract user ID from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var createdByUserId))
            {
                return Unauthorized(new { status = 401, message = "User identity not found or invalid." });
            }


            // Make sure there's at least one other participant
            if (dto.ParticipantUserIds == null || dto.ParticipantUserIds.Count < 1)
                return BadRequest(new { status = 400, message = "At least one other participant is required." });

            // Add the creator (current user) to the participant list
            var participantIds = dto.ParticipantUserIds
                .Concat(new[] { createdByUserId })
                .Distinct()
                .ToList();

            // Prevent duplicate 1-to-1 chats
            if (!dto.IsGroup)
            {
                if (participantIds.Count != 2)
                    return BadRequest(new { status = 400, message = "1-to-1 chat must have exactly 1 other participant." });

                var user1 = participantIds[0];
                var user2 = participantIds[1];

                var existingChat = await _context.Chats
                    .Include(c => c.Participants)
                    .Where(c => !c.IsGroup)
                    .Where(c => c.Participants.Count == 2 &&
                                c.Participants.Any(p => p.UserId == user1) &&
                                c.Participants.Any(p => p.UserId == user2))
                    .FirstOrDefaultAsync();

                if (existingChat != null)
                {
                    return Ok(new { ChatId = existingChat.Id, Message = "1-to-1 chat already exists." });
                }
            }

            // Handle optional group profile picture
            string imageUrl = null;
            if (dto.IsGroup && dto.GroupProfilePicture != null && dto.GroupProfilePicture.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(dto.GroupProfilePicture.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                    return BadRequest(new { status = 400, message = "Only .jpg, .jpeg, and .png files are allowed." });

                var uploadsFolder = Path.Combine("wwwroot", "images", "group-profiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.GroupProfilePicture.CopyToAsync(stream);
                }

                imageUrl = $"/images/group-profiles/{fileName}";
            }

            // Create Chat
            var chat = new Chat
            {
                IsGroup = dto.IsGroup,
                GroupName = dto.IsGroup ? dto.GroupName : null,
                GroupProfilePicture = imageUrl,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            // Add participants
            var participants = participantIds.Select(uid => new ChatParticipant
            {
                ChatId = chat.Id,
                UserId = uid,
                IsAdmin = dto.IsGroup && uid == createdByUserId,
                JoinedAt = DateTime.UtcNow
            }).ToList();

            _context.ChatParticipants.AddRange(participants);
            await _context.SaveChangesAsync();

            return Ok(new { ChatId = chat.Id, Message = "Chat created successfully." });
        }
    }
}