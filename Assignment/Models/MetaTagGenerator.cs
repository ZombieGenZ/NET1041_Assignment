using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment.Models
{
    public class MetaTagGenerator
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public long Discount { get; set; }
        public long PreparationTime { get; set; }
        public long Calories { get; set; }
        public string Ingredients { get; set; }
        public bool IsSpicy { get; set; }
        public bool IsVegetarian { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }

        public override string ToString()
        {
            var prompt = $@"
Bạn là chuyên gia SEO chuyên về website đồ ăn nhanh. Hãy tạo một chuỗi HTML meta tags hoàn chỉnh và tối ưu SEO dựa trên thông tin món ăn được cung cấp.

Dữ liệu đầu vào:
Tên món: {Name}
Mô tả: {Description}
Giá: {Price:N0} VND
Giảm giá: {Discount}%
Thời gian chuẩn bị: {PreparationTime} phút
Calories: {Calories}
Nguyên liệu: {Ingredients}
Món cay: {(IsSpicy ? "Có" : "Không")}
Món chay: {(IsVegetarian ? "Có" : "Không")}
Danh mục: {CategoryName}
Mô tả danh mục: {CategoryDescription}

Yêu cầu tạo meta tags:

1. Meta Title (50-60 ký tự):
- Chứa tên món ăn và từ khóa chính
- Thêm tên thương hiệu (có thể dùng ""FastFood Express"")
- Mô tả hấp dẫn, thúc đẩy hành động
- Có thể nhấn mạnh điểm đặc biệt (cay, chay, giảm giá...)

2. Meta Description (150-160 ký tự):
- Mô tả chi tiết món ăn và nguyên liệu
- Nhấn mạnh lợi ích (calories, thời gian chuẩn bị, ...)
- Có call-to-action như ""Đặt ngay"", ""Thử ngay hôm nay""
- Đề cập đến giảm giá nếu có

3. Các meta tags khác cần thiết:
- Meta charset UTF-8
- Meta viewport responsive
- Meta robots (index, follow)

Định dạng output mong muốn:
Trả về code HTML hoàn chỉnh, sẵn sàng sử dụng, chỉ bao gồm các thẻ <meta>, không thêm comment hay giải thích

Lưu ý đặc biệt:
- Tối ưu cho SEO địa phương (có thể thêm ""Hà Nội"", ""TP.HCM""...)
- Sử dụng từ khóa tiếng Việt tự nhiên
- Nhấn mạnh tính độc đáo của món ăn
- Tạo cảm giác thèm ăn qua từ ngữ mô tả
- Đảm bảo không spam từ khóa

Hãy tạo meta tags chuyên nghiệp và hấp dẫn nhất có thể!
";

            return prompt;
        }
    }
}
