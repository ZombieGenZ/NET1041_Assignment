using System.Text.RegularExpressions;

namespace Assignment.Utilities
{
    public static class PasswordChecker
    {
        public static bool CheckStrongPassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                errorMessage = "Không được để trống mật khẩu";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Mật khẩu phải có ít nhất 8 ký tự";
                return false;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "Mật khẩu phải chứa ít nhất một chữ cái viết hoa";
                return false;
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errorMessage = "Mật khẩu phải chứa ít nhất một chữ cái viết thường";
                return false;
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                errorMessage = "Mật khẩu phải chứa ít nhất một chữ số";
                return false;
            }

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+{}\[\]:;<>,.?~\\-]"))
            {
                errorMessage = "Mật khẩu phải chứa ít nhất một ký tự đặc biệt";
                return false;
            }

            errorMessage = "Mật khẩu mạnh";
            return true;
        }
    }
}
