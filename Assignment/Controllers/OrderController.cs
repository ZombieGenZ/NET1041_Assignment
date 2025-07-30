using Assignment.Enum;
using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("orders")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Index([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedTime)
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

            var orders = query.ToList();

            searchPurchaseHistoryModel.Data = orders;

            return View(searchPurchaseHistoryModel);
        }
        [HttpGet]
        [Route("history")]
        [Authorize]
        public IActionResult PurchaseHistory([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] OrderStatus? status)
        {
            long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedTime)
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

            query = query.Where(o => o.UserId == userId);
            var orders = query.ToList();

            searchPurchaseHistoryModel.Data = orders;

            return View(searchPurchaseHistoryModel);
        }
        [HttpGet]
        [Route("delivery")]
        [Authorize(Policy = "ShipperPolicy")]
        public IActionResult Delivery([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedTime)
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

            query = query.Where(o => o.Status == OrderStatus.Confirmed);
            var orders = query.ToList();

            searchPurchaseHistoryModel.Data = orders;

            return View(searchPurchaseHistoryModel);
        }
        [HttpGet]
        [Route("recevied")]
        [Authorize(Policy = "ShipperPolicy")]
        public IActionResult Recevied([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] OrderStatus? status)
        {
            long? userId = CookieAuthHelper.GetUserId(HttpContext.User);
            string? userRole = CookieAuthHelper.GetRole(HttpContext.User);

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Shipper)
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

            query = query.Where(o => o.Status == OrderStatus.Delivery || o.Status == OrderStatus.Completed);

            if (userRole == "Shipper")
            {
                query = query.Where(o => o.ShipperId == userId);
            }

            var orders = query.ToList();

            searchPurchaseHistoryModel.Data = orders;

            return View(searchPurchaseHistoryModel);
        }
    }
}
