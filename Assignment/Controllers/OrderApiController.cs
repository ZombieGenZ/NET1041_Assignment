using System.Diagnostics.Eventing.Reader;
using Assignment.Enum;
using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class OrderApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("api/orders")]
        [Authorize]
        public IActionResult Orders([FromBody] OrderBase order)
        {
            try
            {
                if (string.IsNullOrEmpty(order.Name) || string.IsNullOrEmpty(order.Phone))
                {
                    List<string> errorMsg = new List<string>();
                    if (string.IsNullOrEmpty(order.Name))
                    {
                        errorMsg.Add("tên người nhận");
                    }

                    if (string.IsNullOrEmpty(order.Phone))
                    {
                        errorMsg.Add("số điện thoại người nhận");
                    }

                    string errMessage = "Vui lòng nhập " + string.Join(", ", errorMsg);
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = errMessage
                    });
                }

                if (!string.IsNullOrEmpty(order.Email) && !EmailValidator.IsValidEmail(order.Email))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Địa chỉ email của người nhận không hợp lệ"
                    });
                }

                if (!PhoneNumberValidator.IsValidPhoneNumber(order.Phone))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số điện thoại của người nhận không hợp lệ"
                    });
                }

                if (order.Items.Count == 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng chọn ít nhất một sản phẩm"
                    });
                }

                if (order.Items.Any(item => item.Quantity <= 0))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số lượng sản phẩm phải lớn hơn 0"
                    });
                }

                var mergedItems = order.Items
                    .GroupBy(i => i.ProductId)
                    .Select(g => new OrderWithQuantity
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                List<Products> productsList = new List<Products>();
                List<long> productErrorWithId = new List<long>();
                List<Products> productErrorWithStock = new List<Products>();
                long totalQuantity = 0;
                double totalPrice = 0;
                foreach (OrderWithQuantity product in mergedItems)
                {
                    var exitingProduct = _context.Products.FirstOrDefault(p => p.Id == product.ProductId);
                    if (exitingProduct == null)
                    {
                        productErrorWithId.Add(product.ProductId);
                    }
                    else
                    {
                        if (exitingProduct.Stock < product.Quantity)
                        {
                            productErrorWithStock.Add(exitingProduct);
                        }
                        else
                        {
                            productsList.Add(exitingProduct);
                            totalQuantity += product.Quantity;
                            totalPrice += exitingProduct.Price * product.Quantity;
                        }
                    }
                }

                if (productErrorWithId.Count > 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Các sản phẩm có ID {string.Join(", ", productErrorWithId)} không tồn tại"
                    });
                }

                if (productErrorWithStock.Count > 0)
                {
                    List<string> productNames = productErrorWithStock.Select(p => p.Name).ToList();
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Các sản phẩm {string.Join(", ", productNames)} không còn đủ hàng trong kho"
                    });
                }

                long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        code = "UNAUTHORIZED",
                        message = "Bạn cần đăng nhập để thực hiện đặt hàng"
                    });
                }

                Orders orderEntity = new Orders
                {
                    Name = order.Name,
                    Email = order.Email,
                    Phone = order.Phone,
                    Status = OrderStatus.Ordered,
                    TotalQuantity = totalQuantity,
                    TotalPrice = totalPrice,
                    OrderTime = DateTime.Now,
                    UserId = userId
                };

                _context.Orders.Add(orderEntity);
                _context.SaveChanges();

                List<OrderDetail> orderDetails = new List<OrderDetail>();

                foreach (var item in productsList)
                {
                    orderDetails.Add(new OrderDetail
                    {
                        OrderId = orderEntity.Id,
                        ProductId = item.Id,
                        PricePreItems = item.Price,
                        TotalQuantityPreItems = mergedItems.First(i => i.ProductId == item.Id).Quantity,
                        TotalPricePreItems = mergedItems.First(i => i.ProductId == item.Id).Quantity * item.Price
                    });
                }

                _context.OrderDetails.AddRange(orderDetails);

                foreach (var item in productsList)
                {
                    item.Stock -= mergedItems.First(i => i.ProductId == item.Id).Quantity;
                    item.Sold += mergedItems.First(i => i.ProductId == item.Id).Quantity;
                }
                _context.Products.UpdateRange(productsList);
                _context.SaveChanges();

                return Json(new
                {
                    code = "ORDER_SUCCESS",
                    message = "Đặt hàng thành công",
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "ORDER_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPut]
        [Route("api/orders/complete/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult CompleteOrder(long id)
        {
            try
            {
                var exitingOrder = _context.Orders.FirstOrDefault(o => o.Id == id && o.Status == OrderStatus.Ordered);
                if (exitingOrder == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Đơn hàng không tồn tại"
                    });
                }

                exitingOrder.Status = OrderStatus.Completed;
                _context.Orders.Update(exitingOrder);
                _context.SaveChanges();

                return Json(new
                {
                    code = "ORDER_COMPLETED_SUCCESS",
                    message = "Đơn hàng đã được hoàn thành"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "ORDER_COMPLETED_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPut]
        [Route("api/orders/cancel/{id:long}")]
        [Authorize]
        public IActionResult CancelOrder(long id)
        {
            try
            {
                var currentUser = HttpContext.User;
                var userId = CookieAuthHelper.GetUserId(currentUser);
                var role = CookieAuthHelper.GetRole(currentUser);

                Orders? exitingOrder;
                if (!string.IsNullOrWhiteSpace(role) && role == "Admin")
                {
                    exitingOrder = _context.Orders.FirstOrDefault(o => o.Id == id && o.Status == OrderStatus.Ordered);
                }
                else
                {
                    exitingOrder = _context.Orders
                        .FirstOrDefault(o => o.Id == id && o.Status == OrderStatus.Ordered && o.UserId == userId);
                }
                    
                if (exitingOrder == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Đơn hàng không tồn tại"
                    });
                }
                exitingOrder.Status = OrderStatus.Cancelled;
                _context.Orders.Update(exitingOrder);
                var orderDetails = _context.OrderDetails.Where(od => od.OrderId == exitingOrder.Id).ToList();
                foreach (var orderDetail in orderDetails)
                {
                    var product = _context.Products.FirstOrDefault(p => p.Id == orderDetail.ProductId);
                    if (product != null)
                    {
                        product.Stock += orderDetail.TotalQuantityPreItems;
                        product.Sold -= orderDetail.TotalQuantityPreItems;
                        _context.Products.Update(product);
                    }
                }
                _context.SaveChanges();
                return Json(new
                {
                    code = "ORDER_CANCELED_SUCCESS",
                    message = "Đơn hàng đã được hủy"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "ORDER_CANCELED_FAILURE",
                    message = e.Message
                });
            }
        }
    }
}
