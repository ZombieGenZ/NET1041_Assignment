using Assignment.Enum;
using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class EvaluateApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EvaluateApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("api/evaluate/check/{id:long}")]
        [Authorize]
        public IActionResult CheckBuyAndEvaluate(long id)
        {
            long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

            if (userId == null)
            {
                return Unauthorized(new
                {
                    code = "UNAUTHORIZED",
                    message = "Bạn cần đăng nhập để thực hiện chức năng này"
                });
            }

            var order = _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
                .SelectMany(o => o.OrderDetails)
                .Any(od => od.ProductId == id);

            if (order)
            {
                return Json(new
                {
                    code = "PURCHASED",
                    message = "Bạn đã mua sản phẩm này"
                });
            }
            return Json(new
            {
                code = "NOT_PURCHASED",
                message = "Bạn chưa mua sản phẩm này"
            });
        }

        [HttpPost]
        [Route("api/evaluate")]
        [Authorize]
        public IActionResult Create([FromBody] EvaluateBase evaluates)
        {
            try
            {
                long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        code = "UNAUTHORIZED",
                        message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    });
                }

                var product = _context.Products.FirstOrDefault(p => p.Id == evaluates.ProductId);

                if (product == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Sản phẩm không tồn tại"
                    });
                }

                if (evaluates.Star < 1 || evaluates.Star > 5)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Bạn chỉ có thể đánh giá từ 1 đến 5 sao"
                    });
                }

                if (_context.Evaluates.Any(e => e.UserId == userId && e.ProductId == evaluates.ProductId))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Bạn đã đánh giá cho sản phẩm này rồi"
                    });
                }

                var newEvaluate = new Evaluates
                {
                    UserId = userId.Value,
                    ProductId = product.Id,
                    Star = evaluates.Star,
                    Content = evaluates.Content
                };
                _context.Evaluates.Add(newEvaluate);

                _context.SaveChanges();

                product.TotalEvaluate = _context.Evaluates.Count(e => e.ProductId == product.Id);
                product.AverageEvaluate = _context.Evaluates
                    .Where(e => e.ProductId == product.Id)
                    .Select(e => e.Star)
                    .AsEnumerable()
                    .DefaultIfEmpty(0)
                    .Average();

                _context.Products.Update(product);

                _context.SaveChanges();

                return Json(new
                {
                    code = "CREATE_SUCCESS",
                    message = "Tạo đánh giá thành công",
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "CREATE_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPut]
        [Route("api/evaluate/{id:long}")]
        [Authorize]
        public IActionResult Update(long id, [FromBody] EvaluateBase2 evaluates)
        {
            try
            {
                long? userId = CookieAuthHelper.GetUserId(HttpContext.User);
                string? role = CookieAuthHelper.GetRole(HttpContext.User);
                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        code = "UNAUTHORIZED",
                        message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    });
                }

                var evaluate = _context.Evaluates.FirstOrDefault(e => e.Id == id && (e.UserId == userId.Value || role == "Admin"));
                if (evaluate == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Đánh giá không tồn tại hoặc bạn không có quyền sửa đánh giá này"
                    });
                }

                if (evaluates.Star < 1 || evaluates.Star > 5)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Bạn chỉ có thể đánh giá từ 1 đến 5 sao"
                    });
                }

                evaluate.Star = evaluates.Star;
                evaluate.Content = evaluates.Content;

                _context.Evaluates.Update(evaluate);

                _context.SaveChanges();

                var product = _context.Products.FirstOrDefault(p => p.Id == evaluate.ProductId);
                if (product != null)
                {
                    product.TotalEvaluate = _context.Evaluates.Count(e => e.ProductId == product.Id);
                    product.AverageEvaluate = _context.Evaluates
                        .Where(e => e.ProductId == product.Id)
                        .Select(e => e.Star)
                        .AsEnumerable()
                        .DefaultIfEmpty(0)
                        .Average();

                    _context.Products.Update(product);

                    _context.SaveChanges();
                }

                return Json(new
                {
                    code = "UPDATE_SUCCESS",
                    message = "Cập nhật đánh giá thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "UPDATE_FAILURE",
                    message = e.Message
                });
            }
        }

        [HttpDelete]
        [Route("api/evaluate/{id:long}")]
        public IActionResult Delete(long id)
        {
            try
            {
                long? userId = CookieAuthHelper.GetUserId(HttpContext.User);
                string? role = CookieAuthHelper.GetRole(HttpContext.User);
                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        code = "UNAUTHORIZED",
                        message = "Bạn cần đăng nhập để thực hiện chức năng này"
                    });
                }

                var evaluate = _context.Evaluates.FirstOrDefault(e => e.Id == id && (e.UserId == userId.Value || role == "Admin"));
                if (evaluate == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Đánh giá không tồn tại hoặc bạn không có quyền xóa đánh giá này"
                    });
                }

                var product = _context.Products.FirstOrDefault(p => p.Id == evaluate.ProductId);

                // Xóa đánh giá
                _context.Evaluates.Remove(evaluate);

                _context.SaveChanges();

                if (product != null)
                {
                    product.TotalEvaluate = _context.Evaluates.Count(e => e.ProductId == product.Id);
                    product.AverageEvaluate = _context.Evaluates
                        .Where(e => e.ProductId == product.Id)
                        .Select(e => e.Star)
                        .AsEnumerable()
                        .DefaultIfEmpty(0)
                        .Average();

                    _context.Products.Update(product);

                    _context.SaveChanges();
                }

                return Json(new
                {
                    code = "DELETE_SUCCESS",
                    message = "Xóa đánh giá thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "DELETE_FAILURE",
                    message = e.Message
                });
            }
        }
    }
}