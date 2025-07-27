using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.Where(p => p.IsPublish).Include(p => p.Category).ToList();

            var productsByCategory = products
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(productsByCategory);
        }

        public IActionResult Cart()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
