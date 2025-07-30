using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;

namespace Assignment.Controllers
{
    public class VoucherController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VoucherController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("vouchers")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("vouchers/my-voucher")]
        [Authorize]
        public IActionResult MyVoucher()
        {
            long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

            var user = _context.Users.Include(users => users.Vouchers).FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user.Vouchers);
        }
    }
}
