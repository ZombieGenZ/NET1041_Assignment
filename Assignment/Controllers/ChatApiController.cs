using Assignment.Models;
using Assignment.Service;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class ChatApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<RealtimeHub> _hubContext;
        public ChatApiController(ApplicationDbContext context, IHubContext<RealtimeHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        [HttpGet]
        [Route("api/chat")]
        public async Task<IActionResult> Get([FromQuery] string? chatid, [FromQuery] long? id)
        {
            var userId = CookieAuthHelper.GetUserId(User);
            var userRole = CookieAuthHelper.GetRole(User);
            if (!userId.HasValue && string.IsNullOrWhiteSpace(chatid) && !id.HasValue)
            {
                return Json(new
                {
                    code = "INPUT_DATA_ERROR",
                    message = "Dử liệu đầu vào không hợp lệ"
                });
            }

            ChatRoom? room = null;

            if (id.HasValue)
            {
                room = _context.ChatRooms.Include(cr => cr.ChatMessages).FirstOrDefault(cr => cr.Id == id.Value);
            }
            else if (userId != null)
            {
                room = _context.ChatRooms.Include(cr => cr.ChatMessages).FirstOrDefault(cr => cr.IsIdentification && cr.UserId == userId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(chatid))
            {
                room = _context.ChatRooms.Include(cr => cr.ChatMessages).FirstOrDefault(cr => !cr.IsIdentification && cr.ChatId == chatid);
            }

            if (room == null)
            {
                if (id.HasValue && userRole == "Admin")
                {
                    return Json(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Yêu cầu dử liệu không hợp lệ"
                    });
                }

                if (userId.HasValue && userRole != "Admin")
                {
                    room = new ChatRoom()
                    {
                        IsIdentification = true,
                        UserId = userId
                    };
                    _context.ChatRooms.Add(room);
                }
                else if (!string.IsNullOrWhiteSpace(chatid))
                {
                    room = new ChatRoom()
                    {
                        ChatId = chatid
                    };
                    _context.ChatRooms.Add(room);
                }

                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group("ChatStaff").SendAsync("NewNotification", new
                {
                    id = room?.Id,
                    notification = false
                });
            }

            if (room != null && userRole == "Admin")
            {
                room.IsRead = true;
                room.UpdatedAt = DateTime.Now;
                _context.ChatRooms.Update(room);
                await _context.SaveChangesAsync();
            }

            return Json(new
            {
                code = "OK",
                roomId = room?.Id,
                data = room?.ChatMessages ?? new List<ChatMessage>()
            });
        }
        [HttpGet]
        [Route("api/chat/list")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult GetList()
        {
            var rooms = _context.ChatRooms.Include(cr => cr.User).OrderByDescending(cr => cr.UpdatedAt).ToList();
            return Json(rooms);
        }

    }
}
