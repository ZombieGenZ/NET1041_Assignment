using Assignment.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Assignment.Controllers
{
    public class ChatController : Controller
    {
        [Route("chat/{id:long?}")]
        public IActionResult Index(long? id)
        {
            var userRole = CookieAuthHelper.GetRole(HttpContext.User);

            if (userRole == "Admin")
            {
                if (id.HasValue)
                {
                    ViewBag.CurrentRoomChat = id;
                }
                else
                {
                    ViewBag.CurrentRoomChat = 0;
                }

                return View("StaffChat");
            }

            return View("UserChat");
        }
    }
}
