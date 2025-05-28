using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dto.Chat;
using api.Models;
using api.Responses;
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatDto dto)
        {
            if (dto.ParticipantUserIds == null || dto.ParticipantUserIds.Count < 2)
                return BadRequest(new ResponseWithStatuscode(400, "At least two participants are required."));

            // For 1-to-1 chat: prevent duplicates
            if (!dto.IsGroup)
            {
                if (dto.ParticipantUserIds.Distinct().Count() != 2)
                    return BadRequest(new ResponseWithStatuscode(400, "1-to-1 chat must have exactly 2 unique participants."));

                var user1 = dto.ParticipantUserIds[0];
                var user2 = dto.ParticipantUserIds[1];

                var existingChat = await _context.Chats
                    .Include(c => c.Participants)
                    .Where(c => !c.IsGroup)
                    .Where(c => c.Participants != null &&
                        c.Participants.Count == 2 &&
                        c.Participants.Any(p => p.UserId == user1) &&
                        c.Participants.Any(p => p.UserId == user2))
                    .FirstOrDefaultAsync();

                if (existingChat != null)
                {
                    return Ok(new
                    {
                        ChatId = existingChat.Id,
                        Message = "1-to-1 chat already exists."
                    });
                }
            }

            // Create new chat
            var chat = new Chat
            {
                IsGroup = dto.IsGroup,
                GroupName = dto.IsGroup ? dto.GroupName : null,
                GroupProfilePicture = dto.IsGroup ? dto.GroupProfilePicture : null,
                CreatedByUserId = dto.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            // Add participants
            var participants = dto.ParticipantUserIds
                .Distinct()
                .Select(uid => new ChatParticipant
                {
                    ChatId = chat.Id,
                    UserId = uid,
                    IsAdmin = dto.IsGroup ? uid == dto.CreatedByUserId : false,
                    JoinedAt = DateTime.UtcNow
                }).ToList();

            _context.ChatParticipants.AddRange(participants);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                chatId = chat.Id,
                message = "Chat created successfully."
            });
        }
    }
}