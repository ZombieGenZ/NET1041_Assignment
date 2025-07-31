using System.Diagnostics.Eventing.Reader;
using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

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
        [HttpGet]
        [Route("api/users/accumulated-points")]
        [Authorize]
        public IActionResult GetAccumulatedPoints()
        {
            long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

            if (userId == null)
            {
                return NotFound();
            }

            return Json(new
            {
                code = "GET_ACCUMULATED_POINTS_SUCCESS",
                points = _context.Users.Find(userId)!.AccumulatedPoints
            });
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
                if (user.DateOfBirth > today)
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

                string code = RandomStringGenerator.GenerateRandomAlphanumericString(10);

                _context.Vouchers.Add(new Vouchers()
                {
                    Code = code,
                    Name = "Voucher cho người dùng mới",
                    Description = "Voucher này được cấp cho người dùng mới đăng ký tài khoản",
                    Type = VoucherTypeEnum.Private,
                    UserId = newUser.Id,
                    DiscountType = DiscountTypeEnum.Percent,
                    Discount = 30,
                    Quantity = 1,
                    StartTime = DateTime.Now,
                    IsLifeTime = false,
                    EndTime = DateTime.Now.AddDays(7),
                    MinimumRequirements = 200000,
                    UnlimitedPercentageDiscount = true
                });

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
                    string subject = $"Chào mừng bạn đến với gia đình {MailForm.Trademark}!";
                    string html = MailForm.Welcome(code);

                    await EmailSender.SendMail(_configuration, user.Email, subject, html);

                    await VerifyAccount(user.Email);
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
        [HttpPost]
        [Route("api/users/verify-account/{email}")]
        public async Task<IActionResult> VerifyAccount(string email)
        {
            try
            {
                var result = _context.Users.Include(users => users.VerifyAccounts)
                    .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

                if (result == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Tài khoản không tồn tại"
                    });
                }

                _context.VerifyAccounts.RemoveRange(result.VerifyAccounts);
                VerifyAccount newVerifyAccount = new VerifyAccount()
                {
                    UserId = result.Id,
                    ExpirationTime = DateTime.Now.AddDays(1)
                };
                _context.VerifyAccounts.Add(newVerifyAccount);
                await _context.SaveChangesAsync();

                _ = Task.Run(async () =>
                {
                    string subject = $"Xác minh toàn khoản {MailForm.Trademark} của bạn";
                    string html = MailForm.VerifyAccount(newVerifyAccount.Token);

                    await EmailSender.SendMail(_configuration, result.Email, subject, html);
                });

                return Ok(new
                {
                    code = "VERIFY_ACCOUNT_SUCCESS",
                    message = "Gửi email xác thực tài khoản thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "VERIFY_ACCOUNT_FAILED",
                    message = e.Message
                });
            }
        }
        [HttpPost]
        [Route("api/users/forgot-password/{email}")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                var result = _context.Users.Include(users => users.ForgotPasswords)
                    .FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

                if (result == null)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Tài khoản không tồn tại"
                    });
                }

                _context.ForgotPasswords.RemoveRange(result.ForgotPasswords);
                ForgotPassword newForgotPassword = new ForgotPassword()
                {
                    UserId = result.Id,
                    ExpirationTime = DateTime.Now.AddDays(1)
                };
                _context.ForgotPasswords.Add(newForgotPassword);
                await _context.SaveChangesAsync();

                _ = Task.Run(async () =>
                {
                    string subject = "Yêu cầu đặt lại mật khẩu";
                    string html = MailForm.ForgotPassword(newForgotPassword.Token);

                    await EmailSender.SendMail(_configuration, result.Email, subject, html);
                });

                return Ok(new
                {
                    code = "FORGOT_PASSWORD_SUCCESS",
                    message = "Gửi email yêu cầu đặt lại mật khẩu thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "FORGOT_PASSWORD_FAILED",
                    message = e.Message
                });
            }
        }
        [HttpGet]
        [Route("api/users/get-user-type")]
        [Authorize]
        public IActionResult GetUserType()
        {
            try
            {
                long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

                if (userId == null)
                {
                    return NotFound();
                }

                var user = _context.Users.Find(userId);

                if (user == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    code = "GET_USER_TYPE_SUCCESS",
                    type = user.UserType
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "GET_USER_TYPE_FAILED",
                    message = e.Message
                });
            }
        }
        [HttpGet]
        [Route("api/users/get-user-password")]
        [Authorize]
        public IActionResult GetUserPassword()
        {
            try
            {
                long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

                if (userId == null)
                {
                    return NotFound();
                }

                var user = _context.Users.Find(userId);

                if (user == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    code = "GET_USER_PASSWORD_SUCCESS",
                    password = user.Password != null
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "GET_USER_PASSWORD_FAILED",
                    message = e.Message
                });
            }
        }
        [HttpPut]
        [Route("api/users/change-infomation")]
        [Authorize]
        public async Task<IActionResult> ChangeInfomation([FromBody] ChangeInfomation infomation)
        {
            try
            {
                long? userId = CookieAuthHelper.GetUserId(HttpContext.User);

                if (userId == null)
                {
                    return NotFound();
                }

                var user = _context.Users.Find(userId);

                if (user == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(infomation.Name) || string.IsNullOrEmpty(infomation.Email) ||
                    string.IsNullOrWhiteSpace(infomation.Phone))
                {
                    List<string> errorMsg = new List<string>();
                    if (string.IsNullOrEmpty(infomation.Name))
                    {
                        errorMsg.Add("tên người dùng");
                    }

                    if (string.IsNullOrEmpty(infomation.Email))
                    {
                        errorMsg.Add("địa chỉ email");
                    }

                    if (string.IsNullOrEmpty(infomation.Phone))
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

                if (!EmailValidator.IsValidEmail(infomation.Email))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Địa chỉ email không hợp lệ"
                    });
                }

                if (!PhoneNumberValidator.IsValidPhoneNumber(infomation.Phone))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số điện thoại không hợp lệ"
                    });
                }

                if (infomation.DateOfBirth > DateTime.Today)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Ngày sinh không hợp lệ"
                    });
                }

                if (_context.Users.Any(u => u.Email == infomation.Email && u.Id != userId))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Địa chỉ email đã được sử dụng"
                    });
                }

                if (_context.Users.Any(u => u.Phone == infomation.Phone && u.Id != userId))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Số điện thoại đã được sử dụng"
                    });
                }

                user.Name = infomation.Name;
                user.Email = infomation.Email.ToLower();
                user.Phone = infomation.Phone;
                user.DateOfBirth = infomation.DateOfBirth;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                await CookieAuthHelper.SignOutUserAsync(HttpContext);
                await CookieAuthHelper.SignInUserAsync(
                    HttpContext,
                    user.Id,
                    user.Name,
                    user.Role,
                    user.Email.ToLower(),
                    user.Phone,
                    user.DateOfBirth
                );

                return Json(new
                {
                    code = "CHANGE_INFOMATION_SUCCESS",
                    message = "Cập nhật thông tin thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "CHANGE_INFOMATION_FAILED",
                    message = e.Message
                });
            }
        }

        [HttpPut]
        [Route("api/users/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            try
            {
                Users? user;
                ForgotPassword? currentForgotPassword = null;
                if (string.IsNullOrWhiteSpace(changePassword.Token))
                {
                    long? userId = CookieAuthHelper.GetUserId(HttpContext.User);
                    if (userId == null)
                    {
                        return NotFound();
                    }

                    user = _context.Users.Include(users => users.ForgotPasswords).FirstOrDefault(u => u.Id == userId);

                    if (user == null)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    var result = _context.ForgotPasswords
                        .Include(fp => fp.User).ThenInclude(users => users.ForgotPasswords)
                        .FirstOrDefault(fp => fp.Token == changePassword.Token);

                    if (result == null || result.ExpirationTime < DateTime.Now)
                    {
                        return NotFound();
                    }

                    currentForgotPassword = result;
                    user = result.User;
                }

                if (string.IsNullOrEmpty(changePassword.NewPassword) ||
                    string.IsNullOrEmpty(changePassword.ConfirmPassword))
                {
                    List<string> errorMsg = new List<string>();
                    if (string.IsNullOrEmpty(changePassword.NewPassword))
                    {
                        errorMsg.Add("mật khẩu mới");
                    }

                    if (string.IsNullOrEmpty(changePassword.ConfirmPassword))
                    {
                        errorMsg.Add("xác nhận mật khẩu mới");
                    }

                    string errMessage = "Vui lòng nhập " + string.Join(", ", errorMsg);
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = errMessage
                    });
                }

                if (currentForgotPassword == null)
                {
                    if (user.Password != null)
                    {
                        if (string.IsNullOrWhiteSpace(changePassword.OldPassword))
                        {
                            return UnprocessableEntity(new
                            {
                                code = "INPUT_DATA_ERROR",
                                message = "Vui lòng nhập mật khẩu cũ"
                            });
                        }

                        if (user.Password != EncryptionHelper.EncryptToSHA512(changePassword.OldPassword))
                        {
                            return UnprocessableEntity(new
                            {
                                code = "INPUT_DATA_ERROR",
                                message = "Mật khẩu không chính xác"
                            });
                        }
                    }
                }


                if (changePassword.NewPassword != changePassword.ConfirmPassword)
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = "Mật khẩu mới và xác nhận mật khẩu không khớp"
                    });
                }

                if (!PasswordChecker.CheckStrongPassword(changePassword.NewPassword, out string errMsg))
                {
                    return UnprocessableEntity(new
                    {
                        code = "INPUT_DATA_ERROR",
                        message = errMsg
                    });
                }

                user.Password = EncryptionHelper.EncryptToSHA512(changePassword.NewPassword);
                _context.Users.Update(user);
                if (currentForgotPassword != null)
                {
                    _context.ForgotPasswords.RemoveRange(user.ForgotPasswords);
                }

                await _context.SaveChangesAsync();

                await CookieAuthHelper.SignOutUserAsync(HttpContext);

                return Json(new
                {
                    code = "CHANGE_PASSWORD_SUCCESS",
                    message = "Cập nhật mật khẩu thành công"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    code = "CHANGE_PASSWORD_FAILED",
                    message = e.Message
                });
            }
        }
    }
}
