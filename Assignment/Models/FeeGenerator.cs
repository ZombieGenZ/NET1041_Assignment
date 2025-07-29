using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Assignment.Models
{
    public class FeeGenerator
    {
        public double LongitudeDelivery { get; set; }
        public double LatitudeDelivery { get; set; }
        public double LongitudeReceiving { get; set; }
        public double LatitudeReceiving { get; set; }
        public override string ToString()
        {
            return $@"
Bạn là một chuyên gia tính toán khoảng cách và chi phí di chuyển. Khi tôi cung cấp hai địa chỉ dưới dạng tọa độ, hãy:

1. Xác định quốc gia của mỗi địa chỉ dựa trên tọa độ
2. Tính quãng đường di chuyển bằng xe máy (tính bằng mét, chỉ lấy phần nguyên) sử dụng công thức Haversine
   - Nếu quãng đường < 1 mét, ghi là 0
3. Tính chi phí:
   - Nếu cùng quốc gia: [số mét] × 0.001 VND
   - Nếu khác quốc gia: [số km] × 0.002 VND + [phí nhập cảnh của quốc gia thứ hai]

Chỉ trả về một số nguyên duy nhất là tổng chi phí, không có bất kỳ văn bản, giải thích, định dạng hay thông tin nào khác.

Dữ liệu:
Kinh độ giao: {LongitudeDelivery}
Vĩ độ giao: {LatitudeDelivery}
Kinh độ nhận: {LongitudeReceiving}
Vĩ độ nhận: {LatitudeReceiving}
";
        }
    }
}
