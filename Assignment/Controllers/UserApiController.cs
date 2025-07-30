using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment.Controllers
{
    public class UserApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public UserApiController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("api/users")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Get()
        {
            return Json(_context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Phone,
                    u.DateOfBirth,
                    u.Rank,
                    u.Role,
                    u.UserType,
                    u.CreatedAt,
                    u.MainProvider
                })
                .ToList());
        }
        [HttpGet]
        [Route("api/users/shipper")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult GetShipper()
        {
            return Json(_context.Users
                .Where(u => u.Role == "Shipper")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Phone,
                    u.DateOfBirth,
                    u.Rank,
                    u.Role,
                    u.UserType,
                    u.CreatedAt,
                    u.MainProvider
                })
                .ToList());
        }

        [HttpPost]
        [Route("api/users/register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterBody user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrWhiteSpace(user.Phone))
                {
                    List<string> errorMsg = new List<string>();
                    if (string.IsNullOrEmpty(user.Name))
                    {
                        errorMsg.Add("tên người dùng");
                    }

                    if (string.IsNullOrEmpty(user.Email))
                    {
                        errorMsg.Add("địa chỉ email");
                    }

                    if (string.IsNullOrEmpty(user.Phone))
                    {
                        errorMsg.Add("số điện thoại");
                    }

                    string errMessage = "Vui lòng nhập " + string.Join(", ", errorMsg);
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = errMessage
                    });
                }

                if (string.IsNullOrWhiteSpace(user.GoogleId) && string.IsNullOrWhiteSpace(user.FacebookId) && string.IsNullOrWhiteSpace(user.GithubId) && string.IsNullOrWhiteSpace(user.DiscordId))
                {
                    if (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.ConfirmPassword))
                    {
                        List<string> errorMsg = new List<string>();
                        if (string.IsNullOrEmpty(user.Password))
                        {
                            errorMsg.Add("mật khẩu");
                        }

                        if (string.IsNullOrEmpty(user.ConfirmPassword))
                        {
                            errorMsg.Add("xác nhận mật khẩu");
                        }

                        string errMessage = "Vui lòng nhập " + string.Join(", ", errorMsg);
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = errMessage
                        });
                    }
                }

                if (!EmailValidator.IsValidEmail(user.Email))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Địa chỉ email không hợp lệ"
                    });
                }

                if (!PhoneNumberValidator.IsValidPhoneNumber(user.Phone))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số điện thoại không hợp lệ"
                    });
                }

                DateTime today = DateTime.Today;
                if (user.DateOfBirth >= today)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Ngày sinh không hợp lệ"
                    });
                }

                //DateTime eighteenYearsAgo = today.AddYears(-18);
                //if (user.DateOfBirth > eighteenYearsAgo)
                //{
                //    return UnprocessableEntity(new
                //    {
                //        code = "INPUT_DATA_ERROR",
                //        message = "Ngày sinh không hợp lệ"
                //    });
                //}

                if (string.IsNullOrWhiteSpace(user.GoogleId) && string.IsNullOrWhiteSpace(user.FacebookId) && string.IsNullOrWhiteSpace(user.GithubId) && string.IsNullOrWhiteSpace(user.DiscordId))
                {
                    if (user.Password != user.ConfirmPassword)
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Mật khẩu và xác nhận mật khẩu không khớp"
                        });
                    }

                    if (!PasswordChecker.CheckStrongPassword(user.Password, out string errMsg))
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = errMsg
                        });
                    }
                }

                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Địa chỉ email đã được sử dụng"
                    });
                }

                if (_context.Users.Any(u => u.Phone == user.Password))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số điện thoại đã được sử dụng"
                    });
                }

                Users? newUser;

                if (!string.IsNullOrWhiteSpace(user.GoogleId))
                {
                    newUser = new Users()
                    {
                        Name = user.Name,
                        Email = user.Email.ToLower(),
                        Phone = user.Phone,
                        DateOfBirth = user.DateOfBirth,
                        GoogleId = user.GoogleId,
                        MainProvider = "Google",
                        UserType = UserTypeEnum.Verified
                    };
                }
                else if (!string.IsNullOrWhiteSpace(user.FacebookId))
                {
                    newUser = new Users()
                    {
                        Name = user.Name,
                        Email = user.Email.ToLower(),
                        Phone = user.Phone,
                        DateOfBirth = user.DateOfBirth,
                        FacebookId = user.FacebookId,
                        MainProvider = "Facebook",
                        UserType = UserTypeEnum.Verified
                    };
                }
                else if (!string.IsNullOrWhiteSpace(user.GithubId))
                {
                    newUser = new Users()
                    {
                        Name = user.Name,
                        Email = user.Email.ToLower(),
                        Phone = user.Phone,
                        DateOfBirth = user.DateOfBirth,
                        GitHubId = user.GithubId,
                        MainProvider = "Github",
                        UserType = UserTypeEnum.Verified
                    };
                }
                else if (!string.IsNullOrWhiteSpace(user.DiscordId))
                {
                    newUser = new Users()
                    {
                        Name = user.Name,
                        Email = user.Email.ToLower(),
                        Phone = user.Phone,
                        DateOfBirth = user.DateOfBirth,
                        DiscordId = user.DiscordId,
                        MainProvider = "Discord",
                        UserType = UserTypeEnum.Verified
                    };
                }
                else
                {
                    newUser = new Users()
                    {
                        Name = user.Name,
                        Email = user.Email.ToLower(),
                        Phone = user.Phone,
                        DateOfBirth = user.DateOfBirth,
                        Password = EncryptionHelper.EncryptToSHA512(user.Password)
                    };
                }

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                await CookieAuthHelper.SignInUserAsync(
                    HttpContext,
                    newUser.Id,
                    newUser.Name,
                    newUser.Role,
                    newUser.Email.ToLower(),
                    newUser.Phone,
                    newUser.DateOfBirth
                );

                _ = Task.Run(async () =>
                {
                    string subject = "Chào mừng đến với KShop";
                    string html =
                        "<div style=\"margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f0f8ff;\">\r\n    <div style=\"max-width: 600px; margin: 0 auto; background-color: #ffffff; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);\">\r\n        <!-- Header -->\r\n        <div style=\"background: linear-gradient(135deg, #1e3a8a, #3b82f6); padding: 40px 30px; text-align: center;\">\r\n            <h1 style=\"color: #ffffff; font-size: 32px; margin: 0; font-weight: bold; text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);\">\r\n                🎉 Chào mừng đến với KShop! 🎉\r\n            </h1>\r\n            <p style=\"color: #e0f2fe; font-size: 18px; margin: 10px 0 0 0; opacity: 0.9;\">\r\n                Nơi mua sắm trực tuyến tuyệt vời nhất\r\n            </p>\r\n        </div>\r\n\r\n        <!-- Content -->\r\n        <div style=\"padding: 40px 30px;\">\r\n            <div style=\"text-align: center; margin-bottom: 30px;\">\r\n                <div style=\"width: 80px; height: 80px; background: linear-gradient(45deg, #2563eb, #1d4ed8); border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 15px rgba(37, 99, 235, 0.3);\">\r\n                    <span style=\"font-size: 36px; color: #ffffff;\">🛍️</span>\r\n                </div>\r\n                <h2 style=\"color: #1e40af; font-size: 24px; margin: 0 0 15px 0; font-weight: bold;\">\r\n                    Xin chào và cảm ơn bạn đã tham gia!\r\n                </h2>\r\n            </div>\r\n\r\n            <div style=\"background-color: #f8fafc; border-left: 4px solid #3b82f6; padding: 20px; margin: 25px 0; border-radius: 0 8px 8px 0;\">\r\n                <p style=\"color: #334155; font-size: 16px; line-height: 1.6; margin: 0;\">\r\n                    Chúng tôi rất vui mừng chào đón bạn gia nhập cộng đồng KShop! Tại đây, bạn sẽ khám phá hàng ngàn sản phẩm chất lượng với giá cả hợp lý và dịch vụ tuyệt vời.\r\n                </p>\r\n            </div>\r\n\r\n            <div style=\"text-align: center; margin: 35px 0;\">\r\n                <a href=\"#\" style=\"background: linear-gradient(135deg, #2563eb, #1d4ed8); color: #ffffff; text-decoration: none; padding: 15px 40px; border-radius: 50px; font-size: 16px; font-weight: bold; display: inline-block; box-shadow: 0 4px 15px rgba(37, 99, 235, 0.4); transition: transform 0.2s;\">\r\n                    🛒 Bắt đầu mua sắm ngay\r\n                </a>\r\n            </div>\r\n\r\n            <div style=\"background-color: #f1f5f9; padding: 20px; border-radius: 10px; margin: 30px 0;\">\r\n                <h4 style=\"color: #1e40af; font-size: 18px; margin: 0 0 15px 0; font-weight: bold;\">\r\n                    📞 Liên hệ hỗ trợ:\r\n                </h4>\r\n                <p style=\"color: #475569; font-size: 14px; line-height: 1.5; margin: 0;\">\r\n                    <strong>Hotline:</strong> 1900-xxxx<br>\r\n                    <strong>Email:</strong> support@kshop.com<br>\r\n                    <strong>Thời gian:</strong> 8:00 - 22:00 hàng ngày\r\n                </p>\r\n            </div>\r\n        </div>\r\n\r\n        <!-- Footer -->\r\n        <div style=\"background-color: #1e40af; padding: 30px; text-align: center;\">\r\n            <p style=\"color: #e0f2fe; font-size: 14px; margin: 0 0 15px 0;\">\r\n                Cảm ơn bạn đã chọn KShop - Nơi mua sắm tin cậy!\r\n            </p>\r\n            <div style=\"border-top: 1px solid #3b82f6; padding-top: 15px; margin-top: 15px;\">\r\n                <p style=\"color: #94a3b8; font-size: 12px; margin: 0;\">\r\n                    © 2025 KShop. Tất cả quyền được bảo lưu.<br>\r\n                    Bạn nhận được email này vì đã đăng ký tài khoản KShop.\r\n                </p>\r\n            </div>\r\n        </div>\r\n    </div>\r\n</div>";

                    await EmailSender.SendMail(_configuration, user.Email, subject, html);
                });

                return Ok(new
                {
                    code = "REGISTER_SUCCESS",
                    message = "Đăng ký thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "REGISTER_FAILED",
                    message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("api/users/login")]
        public async Task<IActionResult> Login([FromBody] UserLoginBody user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.EmailOrPhone) || string.IsNullOrEmpty(user.Password))
                {
                    List<string> errorMsg = new List<string>();
                    if (string.IsNullOrEmpty(user.EmailOrPhone))
                    {
                        errorMsg.Add("địa chỉ email hoặc số diện thoại");
                    }

                    if (string.IsNullOrEmpty(user.Password))
                    {
                        errorMsg.Add("mật khẩu");
                    }

                    string errMessage = "Vui lòng nhập " + string.Join(", ", errorMsg);
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = errMessage
                    });
                }

                if (!EmailValidator.IsValidEmail(user.EmailOrPhone))
                {
                    if (!PhoneNumberValidator.IsValidPhoneNumber(user.EmailOrPhone))
                    {
                        return UnprocessableEntity(new
                        {
                            code = "INPUT_DATA_ERROR",
                            message = "Địa chỉ email hoặc số điện thoại không hợp lệ"
                        });
                    }
                }

                Users? userEntity = _context.Users.FirstOrDefault(u =>
                    (u.Email.ToLower() == user.EmailOrPhone.ToLower() || u.Phone == user.EmailOrPhone) &&
                    u.Password == EncryptionHelper.EncryptToSHA512(user.Password));

                if (userEntity == null)
                {
                    return Unauthorized(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Địa chỉ email hoặc mật khẩu không chính xác"
                    });
                }

                await CookieAuthHelper.SignInUserAsync(
                    HttpContext,
                    userEntity.Id,
                    userEntity.Name,
                    userEntity.Role,
                    userEntity.Email.ToLower(),
                    userEntity.Phone,
                    userEntity.DateOfBirth
                );

                return Ok(new
                {
                    code = "LOGIN_SUCCESS",
                    message = "Đăng nhập thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "LOGIN_FAILED",
                    message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("api/users/logout")]
        public async Task<IActionResult> Logout()
        {
            await CookieAuthHelper.SignOutUserAsync(HttpContext);

            //return Ok(new
            //{
            //    code = "LOGOUT_SUCCESS",
            //    message = "Bạn đã đăng xuất thành công."
            //});
            return RedirectToAction("Index", "Home");
        }
    }
}
