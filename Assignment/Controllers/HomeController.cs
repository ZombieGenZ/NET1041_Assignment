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

        public IActionResult Index([FromQuery] string? text, [FromQuery] double? min, [FromQuery] double? max)
        {
            var query = _context.Products.AsQueryable();

            SearchProductViewModel searchProductViewModel = new SearchProductViewModel();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var searchTerms = text.Split('+', StringSplitOptions.RemoveEmptyEntries);

                foreach (var term in searchTerms)
                {
                    var lowerCaseTerm = term.ToLower();

                    query = query.Where(p => p.Name.ToLower().Contains(lowerCaseTerm) || p.Description.ToLower().Contains(lowerCaseTerm));
                }

                searchProductViewModel.Text = text.Replace("+", " ");
            }

            if (min.HasValue)
            {
                query = query.Where(p => p.Price >= min.Value);
                searchProductViewModel.Min = min.Value;
            }

            if (max.HasValue)
            {
                query = query.Where(p => p.Price <= max.Value);
                searchProductViewModel.Max = max.Value;
            }

            var products = query.Include(p => p.Category).ToList();

            var productsByCategory = products
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            searchProductViewModel.Data = productsByCategory;

            return View(searchProductViewModel);
        }

        [HttpGet]
        [Route("cart")]
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
