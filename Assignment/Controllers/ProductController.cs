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
                query = query.Where(o => o.CreatedTime >= start.Value && o.CreatedTime <= end.Value);
                searchPurchaseHistoryModel.StartTime = start.Value;
                searchPurchaseHistoryModel.EndTime = end.Value;
            }
            else
            {
                if (start.HasValue)
                {
                    query = query.Where(o => o.CreatedTime >= start.Value);
                    searchPurchaseHistoryModel.StartTime = start.Value;
                }

                if (end.HasValue)
                {
                    query = query.Where(o => o.CreatedTime <= end.Value);
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
