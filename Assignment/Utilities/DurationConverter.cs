using System.Text.RegularExpressions;

namespace Assignment.Utilities
{
    public class DurationConverter
    {
        private static readonly Regex _durationRegex = new Regex(@"^(\d+)([smhdw]|mo|y)$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Kiểm tra xem chuỗi đầu vào có đúng định dạng <số><loại> hay không.
        /// Ví dụ: "1s", "5m", "2h", "10d", "3w", "1mo", "2y".
        /// </summary>
        /// <param name="input">Chuỗi cần kiểm tra.</param>
        /// <returns>True nếu chuỗi hợp lệ, ngược lại là False.</returns>
        public static bool IsValidDurationString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            return _durationRegex.IsMatch(input);
        }

        /// <summary>
        /// Chuyển đổi chuỗi định dạng <số><loại> thành một đối tượng DateTime.
        /// Thời gian trả về là thời gian hiện tại cộng thêm khoảng thời gian được chỉ định.
        /// Nếu chuỗi không hợp lệ, sẽ ném ra ngoại lệ ArgumentException.
        /// </summary>
        /// <param name="input">Chuỗi cần chuyển đổi (ví dụ: "1d", "2mo").</param>
        /// <returns>Đối tượng DateTime biểu thị thời gian trong tương lai.</returns>
        /// <returns>null</returns>
        public static DateTime? ConvertDurationToDateTime(string input)
        {
            if (!IsValidDurationString(input))
            {
                return null;
            }

            Match match = _durationRegex.Match(input);
            int value = int.Parse(match.Groups[1].Value);
            string type = match.Groups[2].Value.ToLower();

            DateTime now = DateTime.Now;

            switch (type)
            {
                case "s":
                    return now.AddSeconds(value);
                case "m":
                    return now.AddMinutes(value);
                case "h":
                    return now.AddHours(value);
                case "d":
                    return now.AddDays(value);
                case "w":
                    return now.AddDays(value * 7);
                case "mo":
                    return now.AddMonths(value);
                case "y":
                    return now.AddYears(value);
                default:
                    return null;
            }
        }
        /// <summary>
        /// Chuyển đổi chuỗi định dạng <số><loại> thành một chuỗi mô tả tiếng Việt.
        /// Ví dụ: "1d" sẽ trả về "1 ngày".
        /// Nếu chuỗi không hợp lệ, sẽ ném ra ngoại lệ ArgumentException.
        /// </summary>
        /// <param name="input">Chuỗi cần chuyển đổi (ví dụ: "1d", "2mo").</param>
        /// <returns>Chuỗi mô tả khoảng thời gian bằng tiếng Việt.</returns>
        public static string ConvertDurationToStringDescription(string input)
        {
            if (!IsValidDurationString(input))
            {
                return "Chuổi không hợp lệ";
            }

            Match match = _durationRegex.Match(input);
            int value = int.Parse(match.Groups[1].Value);
            string type = match.Groups[2].Value.ToLower();

            string unit;
            switch (type)
            {
                case "s":
                    unit = "giây";
                    break;
                case "m":
                    unit = "phút";
                    break;
                case "h":
                    unit = "giờ";
                    break;
                case "d":
                    unit = "ngày";
                    break;
                case "w":
                    unit = "tuần";
                    break;
                case "mo":
                    unit = "tháng";
                    break;
                case "y":
                    unit = "năm";
                    break;
                default:
                    return "Chuỗi không hợp lệ";
            }

            return $"{value} {unit}";
        }
    }
}
