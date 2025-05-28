using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IChatRepository
    {
        Task<Chat?> OneToOneChatExistsAsync(int user1Id, int user2Id);
        Task<string> SaveGroupProfilePictureAsync(IFormFile file);
        Task<Chat> CreateChatAsync(Chat chat, IEnumerable<int> participantIds, int createdByUserId, bool isGroup);
    }
}