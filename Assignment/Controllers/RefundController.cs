using Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment.Controllers
{
    public class RefundController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RefundController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("refund")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Index([FromQuery] DateTime? start)
        {
            return View(_context.Refunds.OrderByDescending(r => r.CreatedAt).ToList());
        }
    }
}
