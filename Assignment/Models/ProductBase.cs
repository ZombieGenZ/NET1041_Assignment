using System.Globalization;
using System.Text.Json.Serialization;

namespace Assignment.Models
{
    public class ProductBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public long Stock { get; set; }
        public long Discount { get; set; }
        public bool IsPublish { get; set; }
        public IFormFile? ProductImage { get; set; }
        public long PreparationTime { get; set; }
        public long Calories { get; set; }
        public string Ingredients { get; set; }
        public bool IsSpicy { get; set; }
        public bool IsVegetarian { get; set; }
        [JsonIgnore]
        public string LongitudeStr { get; set; } = "0";
        [JsonIgnore]
        public string LatitudeStr { get; set; } = "0";
        public double Longitude
        {
            get => double.Parse(LongitudeStr, CultureInfo.InvariantCulture);
            set => LongitudeStr = value.ToString(CultureInfo.InvariantCulture);
        }
        public double Latitude
        {
            get => double.Parse(LatitudeStr, CultureInfo.InvariantCulture);
            set => LatitudeStr = value.ToString(CultureInfo.InvariantCulture);
        }
        public long CategoryId { get; set; }
    }
}
