using System.Text;

namespace Assignment.Utilities
{
    public static class RandomStringGenerator
    {
        /// <summary>
        /// Tạo một chuỗi ngẫu nhiên chứa chữ cái (IN HOA) và số.
        /// </summary>
        /// <param name="length">Độ dài của chuỗi ngẫu nhiên. Mặc định là 20.</param>
        /// <returns>Một chuỗi ngẫu nhiên.</returns>
        public static string GenerateRandomAlphanumericString(int length = 20)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}
