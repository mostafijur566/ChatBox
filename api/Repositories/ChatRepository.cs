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
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Chat> CreateChatAsync(Chat chat, IEnumerable<int> participantIds, int createdByUserId, bool isGroup)
        {
            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            var participants = participantIds.Select(uid => new ChatParticipant
            {
                ChatId = chat.Id,
                UserId = uid,
                IsAdmin = isGroup && uid == createdByUserId,
                JoinedAt = DateTime.UtcNow
            }).ToList();

            await _context.ChatParticipants.AddRangeAsync(participants);
            await _context.SaveChangesAsync();

            return chat;
        }

        public async Task<Chat?> OneToOneChatExistsAsync(int user1Id, int user2Id)
        {
            return await _context.Chats
                    .Include(c => c.Participants)
                    .Where(c => !c.IsGroup)
                    .Where(c => c.Participants != null &&
                                c.Participants.Count == 2 &&
                                c.Participants.Any(p => p.UserId == user1Id) &&
                                c.Participants.Any(p => p.UserId == user2Id))
                    .FirstOrDefaultAsync();
        }

        public async Task<string> SaveGroupProfilePictureAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException("Invalid file extension");

            var uploadsFolder = Path.Combine("wwwroot", "images", "group-profiles");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + ext;
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/images/group-profiles/{fileName}";
        }
    }
}