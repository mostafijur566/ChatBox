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
        Task<List<int>> GetChatIdsAsync(int userId);
        Task<bool> IsUserInChatAsync(int chatId, int userId);
        Task SaveMessageAsync(Message message);
        Task SaveMessageStatusAsync(int chatId, int userId, Message message);
        Task<MessageStatus?> GetMessageStatusAsync(int messageId, int userId);
    }
}