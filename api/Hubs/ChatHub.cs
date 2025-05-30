using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId != null)
            {
                // Get all chats the user belongs to
                var chatIds = await _context.ChatParticipants
                    .Where(cp => cp.UserId == userId)
                    .Select(cp => cp.ChatId)
                    .ToListAsync();

                // Add connection to all relevant chat groups
                foreach (var chatId in chatIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
                }
            }
            await base.OnConnectedAsync();
        }

        public async Task JoinChat(int chatId)
        {
            var userId = GetUserId();
            if (userId == null) return;
            // Verify user is part of the chat
            var isParticipant = await _context.ChatParticipants
                .AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

            if (isParticipant)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
                // await Clients.Group(chatId.ToString())
                //     .SendAsync("UserJoined", userId, DateTime.UtcNow);
            }
        }

        public async Task SendMessage(int chatId, string content, string messageType = "text", string? mediaUrl = null)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                throw new HubException("User not authenticated");
            }

            // var user = await _context.Users
            //     .Where(u => u.Id == userId)
            //     .Select(u => new { u.Id, u.Username })
            //     .FirstOrDefaultAsync();

            // if (user == null) throw new HubException("User not found");

            // Verify user is part of the chat
            var isParticipant = await _context.ChatParticipants
                .AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

            if (!isParticipant)
            {
                throw new HubException("User is not part of this chat");
            }

            // // Create message object (not saved to DB)
            // var message = new
            // {
            //     ChatId = chatId,
            //     SenderId = userId,
            //     SenderName = user.Username,
            //     Content = content,
            //     Timestamp = DateTime.UtcNow
            // };

            // Create and save message
            var message = new Message
            {
                ChatId = chatId,
                SenderId = userId.Value,
                Content = content,
                MessageType = messageType,
                MediaUrl = mediaUrl,
                Timestamp = DateTime.UtcNow,
                IsDeletedForEveryone = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Create initial statuses for all participants
            var participants = await _context.ChatParticipants
                .Where(cp => cp.ChatId == chatId)
                .Select(cp => cp.UserId)
                .ToListAsync();

            foreach (var participantId in participants)
            {
                var status = new MessageStatus
                {
                    MessageId = message.Id,
                    UserId = participantId,
                    Status = participantId == userId ? "sent" : "pending",
                    UpdatedAt = DateTime.UtcNow
                };
                _context.MessageStatuses.Add(status);
            }
            await _context.SaveChangesAsync();

            // Get message with sender info
            var messageDto = await _context.Messages
                .Where(m => m.Id == message.Id)
                .Select(m => new
                {
                    m.Id,
                    m.ChatId,
                    m.SenderId,
                    SenderName = m.Sender.Username,
                    m.Content,
                    m.MessageType,
                    m.MediaUrl,
                    m.Timestamp,
                    Statuses = m.Statuses.Select(s => new
                    {
                        s.UserId,
                        s.Status
                    })
                })
                .FirstOrDefaultAsync();

            // Broadcast to all chat participants
            await Clients.Group(chatId.ToString())
                .SendAsync("ReceiveMessage", messageDto);

            // Update status to "delivered" for recipients
            await UpdateMessageStatus(message.Id, userId.Value, "delivered", false);
        }

        public async Task UpdateMessageStatus(int messageId, int userId, string status, bool isSelfUpdate = true)
        {
            var messageStatus = await _context.MessageStatuses
                .FirstOrDefaultAsync(ms => ms.MessageId == messageId && ms.UserId == userId);

            if (messageStatus != null)
            {
                messageStatus.Status = status;
                messageStatus.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Notify sender if status was updated by recipient
                if (!isSelfUpdate)
                {
                    var message = await _context.Messages
                        .FirstOrDefaultAsync(m => m.Id == messageId);

                    if (message != null)
                    {
                        await Clients.User(message.SenderId.ToString())
                            .SendAsync("MessageStatusUpdated", messageId, userId, status);
                    }
                }
            }
        }

        public async Task DeleteMessageForEveryone(int messageId)
        {
            var userId = GetUserId();
            if (userId == null) return;

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId);

            if (message != null)
            {
                message.IsDeletedForEveryone = true;
                await _context.SaveChangesAsync();

                await Clients.Group(message.ChatId.ToString())
                    .SendAsync("MessageDeleted", messageId);
            }
        }

        public async Task SendTypingIndicator(int chatId, bool isTyping)
        {
            var userId = GetUserId();
            if (userId == null) return;

            var isParticipant = await _context.ChatParticipants
                .AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

            if (isParticipant)
            {
                await Clients.OthersInGroup(chatId.ToString())
                    .SendAsync("UserTyping", userId, isTyping);
            }
        }

        public async Task EditMessage(int messageId, string newContent)
        {
            var userId = GetUserId();
            if (userId == null) return;

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId);

            if (message == null) return;

            message.Content = newContent;
            message.Timestamp = DateTime.UtcNow; // Update timestamp for edit
            await _context.SaveChangesAsync();

            await Clients.Group(message.ChatId.ToString())
                .SendAsync("MessageEdited", messageId, newContent);
        }

        

        private int? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return null;
            }
            return userId;
        }
    }
}