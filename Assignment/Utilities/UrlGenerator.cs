using Assignment.Models;

namespace Assignment.Utilities
{
    public static class UrlGenerator
    {
        public static string GenerateUrl(ApplicationDbContext context, string str, string? oldUrl = null)
        {
            string firstUrl = StringConverter.ConvertUrl(str);

            if (oldUrl != null)
            {
                if (oldUrl == firstUrl)
                {
                    return oldUrl;
                }
            }

            if (string.IsNullOrEmpty(firstUrl))
            {
                return NumberGenerator.RandomNumber().ToString();
            }

            if (context.Products.FirstOrDefault(p => p.Path == firstUrl) == null)
            {
                return firstUrl;
            }

            string secondUrl = $"{firstUrl}-{DateTime.Now.Year}";

            if (context.Products.FirstOrDefault(p => p.Path == secondUrl) == null)
            {
                return secondUrl;
            }

            string finalUrl = $"{secondUrl}-{NumberGenerator.RandomNumber()}";

            return finalUrl;
        }
    }
}
