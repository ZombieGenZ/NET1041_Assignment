using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment.Controllers
{
    public class RedeemApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RedeemApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("api/redeems")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Get([FromQuery] string? text)
        {
            try
            {
                var query = _context.Redeems
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var searchTerms = text.Split('+', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var term in searchTerms)
                    {
                        var lowerCaseTerm = term.ToLower();
                        query = query.Where(r =>
                            r.Name.ToLower().Contains(lowerCaseTerm) ||
                            r.Description.ToLower().Contains(lowerCaseTerm));
                    }
                }

                return Json(query.ToList());
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "GET_REDEEM_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPost]
        [Route("api/redeems")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Create([FromBody] RedeemBase redeem)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(redeem.Name) || string.IsNullOrWhiteSpace(redeem.Description))
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(redeem.Name))
                    {
                        errors.Add("tên mã giảm giá");
                    }

                    if (string.IsNullOrWhiteSpace(redeem.Description))
                    {
                        errors.Add("mô tả mã giảm giá");
                    }

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Vui lòng nhập {string.Join(", ", errors)}"
                    });
                }

                if (!System.Enum.IsDefined(typeof(DiscountTypeEnum), redeem.DiscountType))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Kiểu giảm giá không hợp lệ"
                    });
                }

                if (redeem.DiscountType == DiscountTypeEnum.Percent)
                {
                    if (redeem.Discount < 0 || redeem.Discount > 100)
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
                    if (redeem.Discount <= 0)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Số tiền giảm giá không được bé hơn 0"
                        });
                    }
                }

                if (!redeem.IsLifeTime)
                {
                    if (redeem.EndTime == null || !DurationConverter.IsValidDurationString(redeem.EndTime))
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Thời gian kết thúc không hợp lệ"
                        });
                    }
                }

                if (redeem.MinimumRequirements < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số tiền giảm tối thiểu phải lớn hơn hoặc bằng 0"
                    });
                }

                if (redeem.DiscountType == DiscountTypeEnum.Percent)
                {
                    if (!redeem.UnlimitedPercentageDiscount)
                    {
                        if (redeem.MaximumPercentageReduction <= 0)
                        {
                            return UnprocessableEntity(new
                            {
                                code = "INPUT_DATA_ERROR",
                                message = "Số tiền giảm tối đa phải lớn hơn 0"
                            });
                        }
                    }
                }

                if (redeem.Price < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số điểm phải bỏ ra để đổi thường phải lớn hơn 0"
                    });
                }

                if (!System.Enum.IsDefined(typeof(UserRankEnum), redeem.RankRequirement))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Yêu cầu cấp bật không hợp lệ"
                    });
                }

                Redeems newRedeem = new Redeems
                {
                    Name = redeem.Name,
                    Description = redeem.Description,
                    Discount = redeem.Discount,
                    DiscountType = redeem.DiscountType,
                    IsLifeTime = redeem.IsLifeTime,
                    EndTime = redeem.IsLifeTime ? null : redeem.EndTime,
                    MinimumRequirements = redeem.MinimumRequirements,
                    UnlimitedPercentageDiscount = redeem.UnlimitedPercentageDiscount,
                    MaximumPercentageReduction = redeem.DiscountType == DiscountTypeEnum.Percent && !redeem.UnlimitedPercentageDiscount ? redeem.MaximumPercentageReduction : null,
                    Price = redeem.Price,
                    RankRequirement = redeem.RankRequirement,
                    IsPublish = redeem.IsPublish
                };

                _context.Redeems.Add(newRedeem);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "CREATE_REDEEM_SUCCESS",
                    message = "Tạo vật phẩm đổi thưởng thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "CREATE_REDEEM_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpPut]
        [Route("api/redeems/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Update(long id, [FromBody] RedeemBase redeem)
        {
            try
            {
                var existingRedeem = _context.Redeems.FirstOrDefault(c => c.Id == id);
                if (existingRedeem == null)
                {
                    return NotFound(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vật phẩm đổi thưởng không tồn tại"
                    });
                }

                if (string.IsNullOrWhiteSpace(redeem.Name) || string.IsNullOrWhiteSpace(redeem.Description))
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(redeem.Name))
                    {
                        errors.Add("tên mã giảm giá");
                    }

                    if (string.IsNullOrWhiteSpace(redeem.Description))
                    {
                        errors.Add("mô tả mã giảm giá");
                    }

                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Vui lòng nhập {string.Join(", ", errors)}"
                    });
                }

                if (!System.Enum.IsDefined(typeof(DiscountTypeEnum), redeem.DiscountType))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Kiểu giảm giá không hợp lệ"
                    });
                }

                if (redeem.DiscountType == DiscountTypeEnum.Percent)
                {
                    if (redeem.Discount < 0 || redeem.Discount > 100)
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
                    if (redeem.Discount <= 0)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Số tiền giảm giá không được bé hơn 0"
                        });
                    }
                }

                if (!redeem.IsLifeTime)
                {
                    if (redeem.EndTime == null || !DurationConverter.IsValidDurationString(redeem.EndTime))
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Thời gian kết thúc không hợp lệ"
                        });
                    }
                }

                if (redeem.MinimumRequirements < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số tiền giảm tối thiểu phải lớn hơn hoặc bằng 0"
                    });
                }

                if (redeem.DiscountType == DiscountTypeEnum.Percent)
                {
                    if (!redeem.UnlimitedPercentageDiscount)
                    {
                        if (redeem.MaximumPercentageReduction <= 0)
                        {
                            return UnprocessableEntity(new
                            {
                                code = "INPUT_DATA_ERROR",
                                message = "Số tiền giảm tối đa phải lớn hơn 0"
                            });
                        }
                    }
                }

                if (redeem.Price < 0)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số điểm phải bỏ ra để đổi thường phải lớn hơn 0"
                    });
                }

                if (!System.Enum.IsDefined(typeof(UserRankEnum), redeem.RankRequirement))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Yêu cầu cấp bật không hợp lệ"
                    });
                }

                existingRedeem.Name = redeem.Name;
                existingRedeem.Description = redeem.Description;
                existingRedeem.Discount = redeem.Discount;
                existingRedeem.DiscountType = redeem.DiscountType;
                existingRedeem.IsLifeTime = redeem.IsLifeTime;
                existingRedeem.EndTime = redeem.IsLifeTime ? null : redeem.EndTime;
                existingRedeem.MinimumRequirements = redeem.MinimumRequirements;
                existingRedeem.UnlimitedPercentageDiscount = redeem.UnlimitedPercentageDiscount;
                existingRedeem.MaximumPercentageReduction = redeem.DiscountType == DiscountTypeEnum.Percent && !redeem.UnlimitedPercentageDiscount ? redeem.MaximumPercentageReduction : null;
                existingRedeem.Price = redeem.Price;
                existingRedeem.RankRequirement = redeem.RankRequirement;
                existingRedeem.IsPublish = redeem.IsPublish;
                existingRedeem.UpdatedTime = DateTime.Now;

                _context.Redeems.Update(existingRedeem);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "UPDATE_REDEEM_SUCCESS",
                    message = "Cập nhật vật phẩm đổi thưởng thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "UPDATE_REDEEM_FAILURE",
                    message = e.Message
                });
            }
        }
        [HttpDelete]
        [Route("api/redeems/{id:long}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var existingRedeem = _context.Redeems.FirstOrDefault(c => c.Id == id);
                if (existingRedeem == null)
                {
                    return NotFound(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Vật phẩm đổi thưởng không tồn tại"
                    });
                }

                _context.Redeems.Remove(existingRedeem);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    code = "DELETE_REDEEM_SUCCESS",
                    message = "Xóa vật phẩm đổi thưởng thành công",
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "DELETE_REDEEM_FAILURE",
                    message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("api/redeems/reward/{id:long}")]
        [Authorize]
        public IActionResult Reward(long id)
        {
            try
            {
                long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

                if (userId == null)
                {
                    return NotFound();
                }

                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                var redeem = _context.Redeems.FirstOrDefault(r => r.Id == id && r.IsPublish);

                if (redeem == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Phần tưởng với ID {id} không tồn tại"
                    });
                }
                if (user == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Người dùng không hợp lệ"
                    });
                }

                if (redeem.RankRequirement == UserRankEnum.Copper)
                {
                    if (user.Rank == UserRankEnum.None)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Bạn chưa đủ cấp độ để quy đổi phần thưởng này"
                        });
                    }
                }
                if (redeem.RankRequirement == UserRankEnum.Silver)
                {
                    if (user.Rank == UserRankEnum.None || user.Rank == UserRankEnum.Copper)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Bạn chưa đủ cấp độ để quy đổi phần thưởng này"
                        });
                    }
                }
                if (redeem.RankRequirement == UserRankEnum.Gold)
                {
                    if (user.Rank == UserRankEnum.None || user.Rank == UserRankEnum.Copper || user.Rank == UserRankEnum.Silver)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Bạn chưa đủ cấp độ để quy đổi phần thưởng này"
                        });
                    }
                }
                if (redeem.RankRequirement == UserRankEnum.Diamond)
                {
                    if (user.Rank == UserRankEnum.None || user.Rank == UserRankEnum.Copper || user.Rank == UserRankEnum.Silver || user.Rank == UserRankEnum.Gold)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Bạn chưa đủ cấp độ để quy đổi phần thưởng này"
                        });
                    }
                }

                if (user.AccumulatedPoints < redeem.Price)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = $"Bạn cần thêm {redeem.Price - user.AccumulatedPoints} điểm tích lũy mới có thể quy đổi phần thưởng này"
                    });
                }

                _context.Vouchers.Add(new Vouchers()
                {
                    Code = RandomStringGenerator.GenerateRandomAlphanumericString(),
                    Name = redeem.Name,
                    Description = redeem.Description,
                    Type = VoucherTypeEnum.Private,
                    UserId = user.Id,
                    Discount = redeem.Discount,
                    DiscountType = redeem.DiscountType,
                    Quantity = 1,
                    StartTime = DateTime.Now,
                    IsLifeTime = redeem.IsLifeTime,
                    EndTime = redeem.IsLifeTime ? null : DurationConverter.ConvertDurationToDateTime(redeem.EndTime ?? ""),
                    MinimumRequirements = redeem.MinimumRequirements,
                    UnlimitedPercentageDiscount = redeem.UnlimitedPercentageDiscount,
                    MaximumPercentageReduction = redeem.UnlimitedPercentageDiscount ? null : redeem.MaximumPercentageReduction
                });
                user.AccumulatedPoints -= redeem.Price;
                _context.Users.Update(user);
                _context.SaveChanges();

                return Json(new
                {
                    code = "REWARD_REDEEM_SUCCESS",
                    message = "Đổi thưởng thành công"
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    code = "REWARD_REDEEM_FAILURE",
                    message = e.Message
                });
            }
        }
    }
}
