using Assignment.Enum;
using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("products")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("products/{path}")]
        public IActionResult Detail(string path)
        {
            var product = _context.Products.Include(p => p.Evaluates).ThenInclude(e => e.User).FirstOrDefault(p => p.Path == path && p.IsPublish);
            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(product);
        }
    }
}
