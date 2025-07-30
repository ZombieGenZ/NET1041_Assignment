using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class RedeemController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RedeemController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("redeems")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("redeems/conversion")]
        [Authorize]
        public IActionResult Conversion()
        {
            return View(_context.Redeems.Where(r => r.IsPublish).ToList());
        }
    }
}
