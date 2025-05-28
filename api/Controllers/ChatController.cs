using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.Dto.Chat;
using api.Interfaces;
using api.Mappers;
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
        private readonly IChatRepository _chatRepo;
        public ChatController(ApplicationDbContext context, IChatRepository chatRepo)
        {
            _context = context;
            _chatRepo = chatRepo;
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

                var existingChat = await _chatRepo.OneToOneChatExistsAsync( participantIds[0], participantIds[1]);
                if (existingChat != null)
                {
                    return Ok(new { ChatId = existingChat.Id, Message = "1-to-1 chat already exists." });
                }
            }

            // Handle optional group profile picture
            string? imageUrl = null;
            if (dto.IsGroup && dto.GroupProfilePicture != null && dto.GroupProfilePicture.Length > 0)
            {
                try
                {
                    imageUrl = await _chatRepo.SaveGroupProfilePictureAsync(dto.GroupProfilePicture);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new ResponseWithStatuscode(400, $"{ex.Message}"));
                }
            }

            // Create Chat
            var chat = dto.ToChatFromCreate(imageUrl, createdByUserId);

            var createdChat = await _chatRepo.CreateChatAsync(chat, participantIds, createdByUserId, dto.IsGroup);

            return Ok(new { ChatId = chat.Id, Message = "Chat created successfully." });
        }
    }
}