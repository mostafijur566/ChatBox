using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.Chat;
using api.Models;

namespace api.Mappers
{
    public static class ChatMapper
    {
        public static Chat ToChatFromCreate(this CreateChatDto dto, string? imageUrl, int userId)
        {
            return new Chat
            {
                IsGroup = dto.IsGroup,
                GroupName = dto.IsGroup ? dto.GroupName : null,
                GroupProfilePicture = imageUrl,
                CreatedByUserId = userId,
            };
        }
    }
}