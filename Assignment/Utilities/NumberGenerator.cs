namespace Assignment.Utilities
{
    public static class NumberGenerator
    {
        private static readonly Random _random = new Random();
        public static int RandomNumber(int min = 0, int max = 1_000_000_000)
        {
            return _random.Next(min, max + 1);
        }
    }
}
