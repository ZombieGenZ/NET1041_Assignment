namespace Assignment.Models
{
    public class SearchProductViewModel
    {
        public string? Text { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public Dictionary<Categories, List<Products>> Data { get; set; }
    }
}
