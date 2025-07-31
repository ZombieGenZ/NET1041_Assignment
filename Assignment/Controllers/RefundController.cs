using Assignment.Enum;
using Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult Index([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var query = _context.Refunds
                .OrderByDescending(o => o.CreatedAt)
                .AsQueryable();

            SearchRefundModel searchRefundModel = new SearchRefundModel();

            if (start.HasValue && end.HasValue && start > end)
            {
                query = query.Where(o => o.CreatedAt >= start.Value && o.CreatedAt <= end.Value);
                searchRefundModel.StartTime = start.Value;
                searchRefundModel.EndTime = end.Value;
            }
            else
            {
                if (start.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= start.Value);
                    searchRefundModel.StartTime = start.Value;
                }

                if (end.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= end.Value);
                    searchRefundModel.EndTime = end.Value;
                }
            }

            var refunds = query.ToList();

            searchRefundModel.Data = refunds;

            return View(searchRefundModel);
        }
    }
}
