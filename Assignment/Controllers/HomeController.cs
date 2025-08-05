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

        public IActionResult Index([FromQuery] string? text, [FromQuery] double? min, [FromQuery] double? max,
                          [FromQuery] string? priceSort, [FromQuery] string? ratingSort,
                          [FromQuery] string? salesSort)
        {
            var query = _context.Products.AsQueryable();
            SearchProductViewModel searchProductViewModel = new SearchProductViewModel();

            if (!string.IsNullOrWhiteSpace(text))
            {
                var searchTerms = text.Split('+', StringSplitOptions.RemoveEmptyEntries);
                foreach (var term in searchTerms)
                {
                    var lowerCaseTerm = term.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(lowerCaseTerm) ||
                                           p.Description.ToLower().Contains(lowerCaseTerm));
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

            searchProductViewModel.PriceSort = priceSort;
            searchProductViewModel.RatingSort = ratingSort;
            searchProductViewModel.SalesSort = salesSort;

            query = query.Include(p => p.Category);

            query = ApplySorting(query, priceSort, ratingSort, salesSort);

            var products = query.ToList();
            var productsByCategory = products
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            searchProductViewModel.Data = productsByCategory;

            return View(searchProductViewModel);
        }

        [NonAction]
        private IQueryable<Products> ApplySorting(IQueryable<Products> query,
                                                string? priceSort,
                                                string? ratingSort,
                                                string? salesSort)
        {
            if (!string.IsNullOrWhiteSpace(priceSort))
            {
                var orderedQuery = priceSort.ToLower() == "asc"
                    ? query.OrderBy(p => p.Price - (p.Price * p.Discount / 100))
                    : query.OrderByDescending(p => p.Price - (p.Price * p.Discount / 100));

                if (!string.IsNullOrWhiteSpace(ratingSort))
                {
                    orderedQuery = ratingSort.ToLower() == "asc"
                        ? orderedQuery.ThenBy(p => p.AverageEvaluate)
                        : orderedQuery.ThenByDescending(p => p.AverageEvaluate);
                }

                if (!string.IsNullOrWhiteSpace(salesSort))
                {
                    orderedQuery = salesSort.ToLower() == "asc"
                        ? orderedQuery.ThenBy(p => p.Sold)
                        : orderedQuery.ThenByDescending(p => p.Sold);
                }

                return orderedQuery;
            }
            else if (!string.IsNullOrWhiteSpace(ratingSort))
            {
                var orderedQuery = ratingSort.ToLower() == "asc"
                    ? query.OrderBy(p => p.AverageEvaluate)
                    : query.OrderByDescending(p => p.AverageEvaluate);

                if (!string.IsNullOrWhiteSpace(salesSort))
                {
                    orderedQuery = salesSort.ToLower() == "asc"
                        ? orderedQuery.ThenBy(p => p.Sold)
                        : orderedQuery.ThenByDescending(p => p.Sold);
                }

                return orderedQuery;
            }
            else if (!string.IsNullOrWhiteSpace(salesSort))
            {
                return salesSort.ToLower() == "asc"
                    ? query.OrderBy(p => p.Sold)
                    : query.OrderByDescending(p => p.Sold);
            }

            return query.OrderBy(p => p.Name);
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
