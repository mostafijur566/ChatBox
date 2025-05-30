using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
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

        public async Task SendMessage(int chatId, string content, string messageType = "text")
        {
            var userId = GetUserId();
            if (userId == null)
            {
                throw new HubException("User not authenticated");
            }

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.Username })
                .FirstOrDefaultAsync();

            if (user == null) throw new HubException("User not found");

            // Verify user is part of the chat
            var isParticipant = await _context.ChatParticipants
                .AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

            if (!isParticipant)
            {
                throw new HubException("User is not part of this chat");
            }

            // Create message object (not saved to DB)
            var message = new
            {
                ChatId = chatId,
                SenderId = userId,
                SenderName = user.Username,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            // Broadcast to all chat participants except sender
           await Clients.OthersInGroup(chatId.ToString()).SendAsync("ReceiveMessage", message);

            // Send back to sender with a different event name if needed
            await Clients.Caller.SendAsync("MessageSent", message);
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