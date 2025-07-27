using System.Text.RegularExpressions;

namespace Assignment.Utilities
{
    public static class PhoneNumberValidator
    {
        /// <summary>
        /// Kiểm tra xem một chuỗi có phải là số điện thoại hợp lệ hay không.
        /// </summary>
        /// <param name="phoneNumber">Chuỗi cần kiểm tra.</param>
        /// <param name="pattern">Mẫu biểu thức chính quy để so khớp. Mặc định là mẫu cho số điện thoại Việt Nam.</param>
        /// <returns>True nếu là số điện thoại hợp lệ, ngược lại là False.</returns>
        public static bool IsValidPhoneNumber(string phoneNumber, string pattern = @"^(0|\+84)(3|5|7|8|9)\d{8}$")
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(phoneNumber, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (ArgumentException ex)
            {
                return false;
            }
        }
    }
}
