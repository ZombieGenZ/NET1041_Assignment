using System.Security.Cryptography;
using System.Text;

namespace Assignment.Utilities
{
    public static class EncryptionHelper
    {
        public static string EncryptToSHA512(string inputString)
        {
            using (SHA512 sha512Hash = SHA512.Create())
            {
                byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(inputString));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
