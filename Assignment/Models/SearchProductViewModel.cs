namespace Assignment.Models
{
    public class SearchProductViewModel
    {
        public string? Text { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string? PriceSort { get; set; }
        public string? RatingSort { get; set; }
        public string? SalesSort { get; set; }
        public Dictionary<Categories, List<Products>> Data { get; set; }
    }
}
