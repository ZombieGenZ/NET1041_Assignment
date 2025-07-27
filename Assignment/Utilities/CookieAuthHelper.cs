using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Assignment.Utilities
{
    public static class CookieAuthHelper
    {
        /// <summary>
        /// Đăng nhập người dùng bằng lược đồ xác thực Cookie ("WebAuth").
        /// Phương thức này sẽ phát hành một cookie xác thực cho trình duyệt của người dùng.
        /// </summary>
        /// <param name="httpContext">HttpContext hiện tại của yêu cầu.</param>
        /// <param name="userId">ID của người dùng.</param>
        /// <param name="name">Tên người dùng.</param>
        /// <param name="role">Vai trò của người dùng.</param>
        /// <param name="email">Email của người dùng.</param>
        /// <param name="phone">Số điện thoại của người dùng.</param>
        /// <param name="dateOfBirth">Ngày sinh của người dùng.</param>
        /// <param name="isPersistent">Có nên duy trì đăng nhập sau khi đóng trình duyệt không.</param>
        /// <param name="expirationMinutes">Thời gian hết hạn của cookie tính bằng phút.</param>
        public static async Task SignInUserAsync(
            HttpContext httpContext,
            long userId,
            string name,
            string role,
            string email,
            string? phone = null,
            DateTime? dateOfBirth = null,
            bool isPersistent = true,
            int expirationMinutes = 30)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Email, email)
        };

            if (!string.IsNullOrEmpty(phone))
            {
                claims.Add(new Claim("Phone", phone));
            }

            if (dateOfBirth.HasValue)
            {
                claims.Add(new Claim("DateOfBirth", dateOfBirth.Value.ToString("yyyy-MM-dd")));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims, "WebAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            };

            await httpContext.SignInAsync("WebAuth", new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        /// <summary>
        /// Đăng xuất người dùng khỏi lược đồ xác thực Cookie ("WebAuth").
        /// Phương thức này sẽ xóa cookie xác thực khỏi trình duyệt.
        /// </summary>
        /// <param name="httpContext">HttpContext hiện tại của yêu cầu.</param>
        public static async Task SignOutUserAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync("WebAuth");
        }

        /// <summary>
        /// Trích xuất User ID từ ClaimsPrincipal của người dùng.
        /// User ID được mong đợi là nằm trong claim ClaimTypes.NameIdentifier.
        /// </summary>
        /// <param name="user">Đối tượng ClaimsPrincipal của người dùng hiện tại.</param>
        /// <returns>User ID dưới dạng long? nếu tìm thấy và chuyển đổi thành công; ngược lại là null.</returns>
        public static long? GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && long.TryParse(claim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Trích xuất tên người dùng (username) từ ClaimsPrincipal của người dùng.
        /// Tên người dùng được mong đợi là nằm trong claim ClaimTypes.Name.
        /// </summary>
        /// <param name="user">Đối tượng ClaimsPrincipal của người dùng hiện tại.</param>
        /// <returns>Tên người dùng dưới dạng string? nếu tìm thấy; ngược lại là null.</returns>
        public static string? GetName(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Trích xuất vai trò (role) của người dùng từ ClaimsPrincipal của người dùng.
        /// Vai trò được mong đợi là nằm trong claim ClaimTypes.Role.
        /// </summary>
        /// <param name="user">Đối tượng ClaimsPrincipal của người dùng hiện tại.</param>
        /// <returns>Vai trò của người dùng dưới dạng string? nếu tìm thấy; ngược lại là null.</returns>
        public static string? GetRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Trích xuất email của người dùng từ ClaimsPrincipal.
        /// Email được mong đợi là nằm trong claim ClaimTypes.Email.
        /// </summary>
        /// <param name="user">Đối tượng ClaimsPrincipal của người dùng hiện tại.</param>
        /// <returns>Email dưới dạng string? nếu tìm thấy; ngược lại là null.</returns>
        public static string? GetEmail(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Trích xuất số điện thoại của người dùng từ ClaimsPrincipal.
        /// Số điện thoại được mong đợi là nằm trong claim "Phone".
        /// </summary>
        /// <param name="user">Đối tượng ClaimsPrincipal của người dùng hiện tại.</param>
        /// <returns>Số điện thoại dưới dạng string? nếu tìm thấy; ngược lại là null.</returns>
        public static string? GetPhone(ClaimsPrincipal user)
        {
            return user.FindFirst("Phone")?.Value;
        }

        /// <summary>
        /// Trích xuất ngày sinh của người dùng từ ClaimsPrincipal.
        /// Ngày sinh được mong đợi là nằm trong claim "DateOfBirth".
        /// </summary>
        /// <param name="user">Đối tượng ClaimsPrincipal của người dùng hiện tại.</param>
        /// <returns>Ngày sinh dưới dạng DateTime? nếu tìm thấy và chuyển đổi thành công; ngược lại là null.</returns>
        public static DateTime? GetDateOfBirth(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("DateOfBirth");
            if (claim != null && DateTime.TryParse(claim.Value, out var dateOfBirth))
            {
                return dateOfBirth;
            }
            return null;
        }
    }
}
