using Assignment.Models;
using Assignment.Utilities;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Data;

namespace Assignment.Service
{
    public class RealtimeHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public RealtimeHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            // Ghi chú:
            // Context.User tương tự HttpContext.User
            // có thể sử dụng để lấy thông tin người dùng đã đăng nhập bằng CookieAuthHelper
            // ví dụ:
            // var userId = CookieAuthHelper.GetUserId(Context.User);
            // để add user vào group
            // await Groups.AddToGroupAsync(Context.ConnectionId, "<group_name>");
            // để remove user khỏi group
            // await Groups.RemoveFromGroupAsync(Context.ConnectionId, "<group_name>");
            // và gửi thông báo đến group
            //await Clients.Group("<group_name>").SendAsync("<tên event>", new
            //{
            //    // ... dữ liệu cần gửi
            //});

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ConnectPaymentTracking(string orderId)
        {
            var userId = CookieAuthHelper.GetUserId(Context.User);
            if (userId.HasValue && !string.IsNullOrEmpty(orderId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
            }
        }
        public async Task ConnectChatTracking(string? chatId, long? id)
        {
            var userId = CookieAuthHelper.GetUserId(Context.User);
            var userRole = CookieAuthHelper.GetRole(Context.User);
            if (!userId.HasValue && string.IsNullOrWhiteSpace(chatId) && !id.HasValue)
            {
                return;
            }

            ChatRoom? room = null;

            if (id.HasValue)
            {
                room = _context.ChatRooms.FirstOrDefault(cr => cr.Id == id);
            }
            else if (userId.HasValue)
            {
                room = _context.ChatRooms.FirstOrDefault(cr => cr.IsIdentification && cr.UserId == userId);
            }
            else if (!string.IsNullOrWhiteSpace(chatId))
            {
                room = _context.ChatRooms.FirstOrDefault(cr => !cr.IsIdentification && cr.ChatId == chatId);
            }

            if (room == null)
            {
                //if (id.HasValue && userRole == "Admin")
                //{
                //    return;
                //}

                //if (userId.HasValue && userRole != "Admin")
                //{
                //    room = new ChatRoom()
                //    {
                //        IsIdentification = true,
                //        UserId = userId
                //    };
                //    _context.ChatRooms.Add(room);
                //}
                //else if (!string.IsNullOrWhiteSpace(chatId))
                //{
                //    room = new ChatRoom()
                //    {
                //        ChatId = chatId
                //    };
                //    _context.ChatRooms.Add(room);
                //}

                //await _context.SaveChangesAsync();

                //await Clients.Group("ChatStaff").SendAsync("NewNotification", new
                //{
                //    id = room?.Id,
                //    notification = false
                //});

                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, room?.Id.ToString() ?? string.Empty);
        }
        public async Task ConnectStaffChatTracking()
        {
            var userId = CookieAuthHelper.GetUserId(Context.User);
            var userRole = CookieAuthHelper.GetRole(Context.User);
            if (userId.HasValue && !string.IsNullOrWhiteSpace(userRole) && userRole == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "ChatStaff");
            }
        }
        public async Task SendMessage(long? id, string? chatId, string message)
        {
            var userId = CookieAuthHelper.GetUserId(Context.User);
            var userRole = CookieAuthHelper.GetRole(Context.User);
            if (!userId.HasValue && string.IsNullOrWhiteSpace(chatId) && !id.HasValue)
            {
                return;
            }

            ChatRoom? room = null;

            if (id.HasValue)
            {
                room = _context.ChatRooms.FirstOrDefault(cr => cr.Id == id);
            }
            else if (userId.HasValue)
            {
                room = _context.ChatRooms.FirstOrDefault(cr => cr.IsIdentification && cr.UserId == userId);
            }
            else if (!string.IsNullOrWhiteSpace(chatId))
            {
                room = _context.ChatRooms.FirstOrDefault(cr => !cr.IsIdentification && cr.ChatId == chatId);
            }

            if (room == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (userRole == "Admin")
            {
                _context.ChatMessages.Add(new ChatMessage
                {
                    ChatRoomId = room.Id,
                    Message = message,
                    IsFromUser = false
                });

                room.UpdatedAt = DateTime.Now;
                _context.ChatRooms.Update(room);

                await Clients.Group(room.Id.ToString()).SendAsync("NewMessage", new
                {
                    room = room?.Id,
                    message,
                    IsFromUser = false
                });
            }
            else
            {
                _context.ChatMessages.Add(new ChatMessage
                {
                    ChatRoomId = room.Id,
                    Message = message,
                    IsFromUser = true
                });

                room.IsRead = false;
                room.UpdatedAt = DateTime.Now;
                _context.ChatRooms.Update(room);

                await Clients.Group(room.Id.ToString()).SendAsync("NewMessage", new
                {
                    room = room?.Id,
                    message,
                    IsFromUser = true
                });

                await Clients.Group("ChatStaff").SendAsync("NewNotification", new
                {
                    id = room?.Id,
                    notification = true
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
