using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Data;

namespace Assignment.Service
{
    [Authorize(AuthenticationSchemes = "WebAuth")]
    public class RealtimeHub : Hub
    {
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
    }
}
