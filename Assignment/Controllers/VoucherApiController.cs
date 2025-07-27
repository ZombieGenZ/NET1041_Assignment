using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Assignment.Controllers
{
    public class VoucherApiController : Controller
    {
        public readonly ApplicationDbContext _context;
        public VoucherApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("api/vouchers")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Get([FromQuery] string? text)
        {
            try
            {
                var query = _context.Vouchers
                    .Include(v => v.User)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var searchTerms = text.Split('+', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var term in searchTerms)
                    {
                        var lowerCaseTerm = term.ToLower();
                        query = query.Where(p =>
                            p.Name.ToLower().Contains(lowerCaseTerm) ||
                            p.Description.ToLower().Contains(lowerCaseTerm) ||
                            p.Code.ToLower().Contains(lowerCaseTerm));
                    }
                }

                var result = query
                    .Select(v => new
                    {
                        v.Id,
                        v.Code,
                        v.Name,
                        v.Description,
                        v.Type,
                        v.UserId,
                        v.Discount,
                        v.DiscountType,
                        v.Used,
                        v.Quantity,
                        v.StartTime,
                        v.IsLifeTime,
                        v.EndTime,
                        v.MinimumRequirements,
                        v.UnlimitedPercentageDiscount,
                        v.MaximumPercentageReduction,
                        User = v.User == null ? null : new
                        {
                            v.User.Name
                        }
                    })
                    .ToList();

                return Json(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "GET_VOUCHER_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPost]
        [Route("api/vouchers")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Create([FromBody] VoucherBase voucher)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(voucher.Name) || string.IsNullOrWhiteSpace(voucher.Description))
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(voucher.Name))
                    {
                        errors.Add("tên mã giảm giá");
                    }

                    if (string.IsNullOrWhiteSpace(voucher.Description))
                    {
                        errors.Add("mô tả mã giảm giá");
                    }

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Vui lòng nhập {string.Join(", ", errors)}"
                    });
                }

                if (!voucher.AutoGeneratorCode)
                {
                    if (string.IsNullOrWhiteSpace(voucher.Code))
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Vui lòng nhập mã giảm giá"
                        });
                    }

                    if (_context.Vouchers.Any(v => v.Code == voucher.Code))
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Mã giảm giá này đã được sử dụng trên một mã giảm giá khác"
                        });
                    }
                }

                if (!System.Enum.IsDefined(typeof(VoucherTypeEnum), voucher.Type))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Kiểu voucher không hợp lệ"
                    });
                }

                Users? users;

                if (voucher.Type == VoucherTypeEnum.Private)
                {
                    users = _context.Users.FirstOrDefault(u => u.Id == voucher.UserId);
                    if (users == null)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "ID người dùng không hợp lệ"
                        });
                    }
                }

                if (!System.Enum.IsDefined(typeof(DiscountTypeEnum), voucher.DiscountType))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Kiểu giảm giá không hợp lệ"
                    });
                }

                if (voucher.DiscountType == DiscountTypeEnum.Percent)
                {
                    if (voucher.Discount < 0 || voucher.Discount > 100)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "% giảm giá chỉ có thể từ 0 đến 100%"
                        });
                    }
                }
                else
                {
                    if (voucher.Discount <= 0)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Số tiền giảm giá không được bé hơn 0"
                        });
                    }
                }

                if (voucher.Quantity <= 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số lượng mã giảm giá không được bé hơn hoặc bằng 0"
                    });
                }

                if (voucher.StartTime == default)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập thời gian bắt đầu hợp lệ"
                    });
                }

                if (!voucher.IsLifeTime)
                {
                    if (!voucher.EndTime.HasValue || voucher.EndTime.Value == default(DateTime))
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Vui lòng nhập thời gian bắt đầu và kết thúc hợp lệ"
                        });
                    }

                    if (voucher.StartTime >= voucher.EndTime.Value)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc"
                        });
                    }
                }

                if (voucher.MinimumRequirements < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số tiền giảm tối thiểu phải lớn hơn hoặc bằng 0"
                    });
                }

                if (voucher.DiscountType == DiscountTypeEnum.Percent)
                {
                    if (!voucher.UnlimitedPercentageDiscount)
                    {
                        if (voucher.MaximumPercentageReduction <= 0)
                        {
                            return UnprocessableEntity(new
                            {
                                code = "INPUT_DATA_ERROR",
                                message = "Số tiền giảm tối đa phải lớn hơn 0"
                            });
                        }
                    }
                }
                
                Vouchers newVouchers = new Vouchers
                {
                    Code = voucher.AutoGeneratorCode ? RandomStringGenerator.GenerateRandomAlphanumericString() : voucher.Code,
                    Name = voucher.Name,
                    Description = voucher.Description,
                    Type = voucher.Type,
                    UserId = voucher.Type == VoucherTypeEnum.Private ? voucher.UserId : null,
                    Discount = voucher.Discount,
                    DiscountType = voucher.DiscountType,
                    Quantity = voucher.Quantity,
                    StartTime = voucher.StartTime,
                    IsLifeTime = voucher.IsLifeTime,
                    EndTime = voucher.IsLifeTime ? null : voucher.EndTime,
                    MinimumRequirements = voucher.MinimumRequirements,
                    UnlimitedPercentageDiscount = voucher.UnlimitedPercentageDiscount,
                    MaximumPercentageReduction = voucher.DiscountType == DiscountTypeEnum.Percent && !voucher.UnlimitedPercentageDiscount ? voucher.MaximumPercentageReduction : null
                };

                _context.Vouchers.Add(newVouchers);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "CREATE_VOUCHER_SUCCESS",
                    message = "Tạo mã giảm giá thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "CREATE_VOUCHER_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPut]
        [Route("api/vouchers/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Update(long id, [FromBody] VoucherBase voucher)
        {
            try
            {
                var existingVoucher = _context.Vouchers.FirstOrDefault(c => c.Id == id);
                if (existingVoucher == null)
                {
                    return NotFound(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Voucher không tồn tại"
                    });
                }

                if (string.IsNullOrWhiteSpace(voucher.Code) || string.IsNullOrWhiteSpace(voucher.Name))
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(voucher.Name))
                    {
                        errors.Add("tên mã giảm giá");
                    }

                    if (string.IsNullOrWhiteSpace(voucher.Description))
                    {
                        errors.Add("mô tả mã giảm giá");
                    }

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Vui lòng nhập {string.Join(", ", errors)}"
                    });
                }

                if (string.IsNullOrWhiteSpace(voucher.Code))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập mã giảm giá"
                    });
                }

                if (_context.Vouchers.Any(v => v.Code == existingVoucher.Code && v.Id != id))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Mã giảm giá này đã được sử dụng trên một mã giảm giá khác"
                    });
                }

                if (!System.Enum.IsDefined(typeof(VoucherTypeEnum), voucher.Type))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Kiểu voucher không hợp lệ"
                    });
                }

                Users? users;

                if (voucher.Type == VoucherTypeEnum.Private)
                {
                    users = _context.Users.FirstOrDefault(u => u.Id == voucher.UserId);
                    if (users == null)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "ID người dùng không hợp lệ"
                        });
                    }
                }

                if (!System.Enum.IsDefined(typeof(DiscountTypeEnum), voucher.DiscountType))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Kiểu giảm giá không hợp lệ"
                    });
                }

                if (voucher.DiscountType == DiscountTypeEnum.Percent)
                {
                    if (voucher.Discount < 0 || voucher.Discount > 100)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "% giảm giá chỉ có thể từ 0 đến 100%"
                        });
                    }
                }
                else
                {
                    if (voucher.Discount <= 0)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Số tiền giảm giá không được bé hơn hoặc bằng 0"
                        });
                    }
                }

                if (voucher.Quantity < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số lượng mã giảm giá không được bé hơn 0"
                    });
                }

                if (voucher.StartTime == default)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vui lòng nhập thời gian bắt đầu hợp lệ"
                    });
                }

                if (!voucher.IsLifeTime)
                {
                    if (!voucher.EndTime.HasValue || voucher.EndTime.Value == default(DateTime))
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Vui lòng nhập thời gian bắt đầu và kết thúc hợp lệ"
                        });
                    }

                    if (voucher.StartTime >= voucher.EndTime.Value)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc"
                        });
                    }
                }

                if (voucher.MinimumRequirements < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số tiền giảm tối thiểu phải lớn hơn hoặc bằng 0"
                    });
                }

                if (voucher.DiscountType == DiscountTypeEnum.Percent)
                {
                    if (!voucher.UnlimitedPercentageDiscount)
                    {
                        if (voucher.MaximumPercentageReduction <= 0)
                        {
                            return UnprocessableEntity(new
                            {
                                code = "INPUT_DATA_ERROR",
                                message = "Số tiền giảm tối đa phải lớn hơn 0"
                            });
                        }
                    }
                }

                existingVoucher.Code = voucher.Code;
                existingVoucher.Name = voucher.Name;
                existingVoucher.Description = voucher.Description;
                existingVoucher.Type = voucher.Type;
                existingVoucher.UserId = voucher.Type == VoucherTypeEnum.Private ? voucher.UserId : null;
                existingVoucher.Discount = voucher.Discount;
                existingVoucher.DiscountType = voucher.DiscountType;
                existingVoucher.Quantity = voucher.Quantity;
                existingVoucher.StartTime = voucher.StartTime;
                existingVoucher.IsLifeTime = voucher.IsLifeTime;
                existingVoucher.EndTime = voucher.IsLifeTime ? null : voucher.EndTime;
                existingVoucher.MinimumRequirements = voucher.MinimumRequirements;
                existingVoucher.UnlimitedPercentageDiscount = voucher.UnlimitedPercentageDiscount;
                existingVoucher.MaximumPercentageReduction = voucher.DiscountType == DiscountTypeEnum.Percent && !voucher.UnlimitedPercentageDiscount
                    ? voucher.MaximumPercentageReduction
                    : null;
                existingVoucher.UpdatedTime = DateTime.Now;
                _context.Vouchers.Update(existingVoucher);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "UPDATE_VOUCHER_SUCCESS",
                    message = "Cập nhật mã giảm giá thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "UPDATE_VOUCHER_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpDelete]
        [Route("api/vouchers/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var existingVoucher = _context.Vouchers.FirstOrDefault(c => c.Id == id);
                if (existingVoucher == null)
                {
                    return NotFound(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Voucher không tồn tại"
                    });
                }

                _context.Vouchers.Remove(existingVoucher);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "DELETE_VOUCHER_SUCCESS",
                    message = "Xóa mã giảm giá thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "DELETE_VOUCHER_FAILURE",
                    message = e.Message
                });
            }
        }
    }
}
