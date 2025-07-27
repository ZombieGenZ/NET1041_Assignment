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
        [HttpGet]
        [Route("products/search")]
        public IActionResult Search([FromQuery] string? text, [FromQuery] double? min, [FromQuery] double? max)
        {
            var query = _context.Products.AsQueryable();

            SearchProductViewModel searchProductViewModel = new SearchProductViewModel();
            bool isSearch = false;

            if (!string.IsNullOrWhiteSpace(text))
            {
                var searchTerms = text.Split('+', StringSplitOptions.RemoveEmptyEntries);

                foreach (var term in searchTerms)
                {
                    var lowerCaseTerm = term.ToLower();

                    query = query.Where(p => p.Name.ToLower().Contains(lowerCaseTerm) || p.Description.ToLower().Contains(lowerCaseTerm));
                }

                searchProductViewModel.Text = text.Replace("+", " ");
                isSearch = true;
            }

            if (min.HasValue)
            {
                query = query.Where(p => p.Price >= min.Value);
                searchProductViewModel.Min = min.Value;
                isSearch = true;
            }

            if (max.HasValue)
            {
                query = query.Where(p => p.Price <= max.Value);
                searchProductViewModel.Max = max.Value;
                isSearch = true;
            }

            var products = query.Include(p => p.Category).ToList();

            var productsByCategory = products
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            searchProductViewModel.Data = productsByCategory;

            if (isSearch)
            {
                return View(searchProductViewModel);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("history")]
        [Authorize]
        public IActionResult PurchaseHistory([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] OrderStatus? status)
        {
            long? userId = CookieAuthHelper.GetUserId(HttpContext.User);
            string? userRole = CookieAuthHelper.GetRole(HttpContext.User);

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .AsQueryable();

            SearchPurchaseHistoryModel searchPurchaseHistoryModel = new SearchPurchaseHistoryModel();

            if (start.HasValue && end.HasValue && start > end)
            {
                query = query.Where(o => o.OrderTime >= start.Value && o.OrderTime <= end.Value);
                searchPurchaseHistoryModel.StartTime = start.Value;
                searchPurchaseHistoryModel.EndTime = end.Value;
            }
            else
            {
                if (start.HasValue)
                {
                    query = query.Where(o => o.OrderTime >= start.Value);
                    searchPurchaseHistoryModel.StartTime = start.Value;
                }

                if (end.HasValue)
                {
                    query = query.Where(o => o.OrderTime <= end.Value);
                    searchPurchaseHistoryModel.EndTime = end.Value;
                }
            }

            if (status.HasValue)
            {
                if (status.Value != (OrderStatus)(-1))
                {
                    query = query.Where(o => o.Status == status.Value);
                    searchPurchaseHistoryModel.Status = status.Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(userRole) && userRole == "Customer")
            {
                query = query.Where(o => o.UserId == userId);
            }

            var orders = query.ToList();

            searchPurchaseHistoryModel.Data = orders;

            return View(searchPurchaseHistoryModel);
        }
    }
}
