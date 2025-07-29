using Assignment.Enum;
using Assignment.Models;
using Assignment.Service;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Eventing.Reader;

namespace Assignment.Controllers
{
    public class OrderApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiApiClient _geminiApiClient;
        private readonly IHubContext<RealtimeHub> _hubContext;
        public OrderApiController(ApplicationDbContext context, GeminiApiClient geminiApiClient, IHubContext<RealtimeHub> hubContext)
        {
            _context = context;
            _geminiApiClient = geminiApiClient;
            _hubContext = hubContext;
        }
        [HttpPost]
        [Route("api/orders")]
        [Authorize]
        public async Task<IActionResult> Orders([FromBody] OrderBase order)
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
                List<ProductListReturnBill> listReturnBills = new List<ProductListReturnBill>();
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
                            listReturnBills.Add(new ProductListReturnBill
                            {
                                Name = exitingProduct.Name,
                                Price = exitingProduct.Price,
                                Quantity = product.Quantity,
                                Discount = exitingProduct.Discount
                            });
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

                double oldPrice = totalPrice;

                if (order.Voucher != null)
                {
                    var voucher = await _context.Vouchers
                        .FirstOrDefaultAsync(v =>
                            v.Code == order.Voucher &&
                            (v.Type == VoucherTypeEnum.Public ||
                             (v.Type == VoucherTypeEnum.Private && v.UserId == userId)) &&
                            v.StartTime <= DateTime.Now &&
                            (v.IsLifeTime || v.EndTime >= DateTime.Now) && 
                            v.Quantity > 0);

                    if (voucher == null)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Mã giảm giá không hợp lệ"
                        });
                    }

                    if (voucher.MinimumRequirements > totalPrice)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = $"Đơn hàng của bạn chưa đủ điều kiện để sử dụng mã giảm giá {voucher.Code}"
                        });
                    }

                    if (voucher.DiscountType == DiscountTypeEnum.Money)
                    {
                        totalPrice -= voucher.Discount;
                    }
                    else if (voucher.DiscountType == DiscountTypeEnum.Percent)
                    {
                        if (voucher.UnlimitedPercentageDiscount)
                        {
                            totalPrice -= totalPrice / 100 * voucher.Discount;
                        }
                        else
                        {
                            if (voucher.MaximumPercentageReduction.HasValue)
                            {
                                double discountAmount = totalPrice / 100 * voucher.Discount;
                                if (discountAmount > voucher.MaximumPercentageReduction.Value)
                                {
                                    discountAmount = voucher.MaximumPercentageReduction.Value;
                                }
                                totalPrice -= discountAmount;
                            }
                            else
                            {
                                totalPrice -= totalPrice / 100 * voucher.Discount;
                            }
                        }
                    }

                    voucher.Used++;
                    voucher.Quantity -= 1;
                    _context.Vouchers.Update(voucher);
                }

                double discount = oldPrice - totalPrice;

                if (totalPrice < 0)
                {
                    totalPrice = 0;
                }

                if (order.Longitude > 180 || order.Latitude > 90 ||
                    order.Longitude < -180 || order.Latitude < -90)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Kinh độ và vĩ độ nhận hàng không hợp lệ"
                    });
                }

                FeeGenerator feeGenerator = new FeeGenerator
                {
                    LatitudeDelivery = 10.793744,
                    LongitudeDelivery = 107.135011,
                    LatitudeReceiving = order.Latitude,
                    LongitudeReceiving = order.Longitude
                };

                string feeStr = await _geminiApiClient.GenerateContentAsync(feeGenerator.ToString());
                
                if (string.IsNullOrEmpty(feeStr) || !double.TryParse(feeStr.Replace("\n", ""), out double fee) || fee < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "FEE_CALCULATION_ERROR",
                        message = "Không thể tính toán phí vận chuyển"
                    });
                }

                double totalBill = totalPrice + fee + totalPrice * 0.1;

                double vat = totalPrice * 0.1;

                Orders orderEntity = new Orders
                {
                    Name = order.Name,
                    Email = order.Email,
                    Phone = order.Phone,
                    Latitude = order.Latitude,
                    Longitude = order.Longitude,
                    Fee = fee,
                    FeeExcludingTax = fee * 0.9,
                    TotalBill = totalBill,
                    Status = OrderStatus.Ordered,
                    TotalQuantity = totalQuantity,
                    TotalPrice = totalPrice,
                    Vat = vat,
                    UserId = userId
                };

                _context.Orders.Add(orderEntity);
                await _context.SaveChangesAsync();

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
                    orderId = $"DH{orderEntity.Id}",
                    productList = listReturnBills,
                    totalPrice,
                    discount,
                    totalBill,
                    accountNo = "0978266980",
                    accountName = "NGUYEN DUC ANH",
                    bankId = "MBBank",
                    vat,
                    fee
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

        [HttpPost]
        [Route("api/orders/checkout")]
        public async Task<IActionResult> CheckOut([FromHeader] string authorization, [FromBody] SeaPayCheckOut body)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authorization))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Apikey không hợp lệ"
                    });
                }

                if (!authorization.StartsWith("Apikey "))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Apikey không hợp lệ"
                    });
                }

                if (authorization.Split(' ')[1] != "&u%A)3v.4ckDX0qFrm6CObIFfJSXIHDTkUWFUD4tLSIEtNOqun")
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Apikey không hợp lệ"
                    });
                }

                if (string.IsNullOrWhiteSpace(body.code))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Nội dung thanh toán không hợp lệ"
                    });
                }

                string orderIdStr = body.code.Substring(2);

                if (!long.TryParse(orderIdStr, out long orderId))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Nội dung thanh toán không hợp lệ"
                    });
                }

                var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Nội dung thanh toán không hợp lệ"
                    });
                }

                if (body.transferAmount < order.TotalBill)
                {
                    // thêm code hoàn tiền sau

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số tiền giao dịch không hợp lệ"
                    });
                }

                order.Status = OrderStatus.Paid;
                order.UpdatedTime = DateTime.Now;

                _context.Update(order);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Groups(body.code).SendAsync("Paid", new
                {
                    ok = true
                });

                return Json(new
                {
                    code = "CHECKOUT_SUCCESS",
                    message = "Thanh toán thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "CHECKOUT_FAILURE",
                    message = e.Message
                });
            }
        }
    }
}
