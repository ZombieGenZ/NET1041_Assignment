namespace Assignment.Utilities
{
    public static class MailForm
    {
        public static string Trademark { get; set; } = "FastFood Express";
        public static string Slogan { get; set; } = "Đồ ăn nhanh - Giao hàng siêu tốc!";
        public static string BaseUrl { get; set; } = "http://localhost";
        public static string UrlDisplay { get; set; } = "www.localhost";
        public static string Hotline { get; set; } = "0999999999";
        public static string Welcome(string voucher)
        {
            return $@"
<div style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f5f5f5;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; box-shadow: 0 4px 8px rgba(0,0,0,0.1);"">
        
        <div style=""background: linear-gradient(135deg, #ff6b35, #ff8c42); padding: 40px 20px; text-align: center; color: white;"">
            <h1 style=""margin: 0; font-size: 32px; font-weight: bold; text-shadow: 2px 2px 4px rgba(0,0,0,0.3);"">
                🍔 {Trademark}
            </h1>
            <p style=""margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;"">
                {Slogan}
            </p>
        </div>

        <div style=""padding: 40px 30px; text-align: center;"">
            <h2 style=""color: #333; font-size: 28px; margin: 0 0 20px 0; line-height: 1.3;"">
                🎉 Chào mừng bạn đến với gia đình {Trademark}!
            </h2>
            
            <p style=""color: #666; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;"">
                Cảm ơn bạn đã tin tưởng và lựa chọn {Trademark}! Chúng tôi rất vui mừng được phục vụ bạn những món ăn nhanh ngon nhất với dịch vụ giao hàng siêu tốc.
            </p>

            <div style=""background: linear-gradient(135deg, #fff3e0, #ffe0b2); border: 2px dashed #ff8c42; border-radius: 15px; padding: 25px; margin: 30px 0; position: relative;"">
                <div style=""background-color: #ff6b35; color: white; padding: 5px 15px; border-radius: 20px; display: inline-block; font-size: 12px; font-weight: bold; margin-bottom: 15px;"">
                    ✨ KHUYẾN MÃI ĐỘC QUYỀN
                </div>
                <h3 style=""color: #d84315; margin: 0 0 10px 0; font-size: 24px;"">
                    GIẢM 30% Đơn đầu tiên!
                </h3>
                <p style=""color: #bf360c; font-size: 16px; margin: 0 0 15px 0; font-weight: 500;"">
                    Sử dụng mã: <span style=""background-color: #ff6b35; color: white; padding: 5px 10px; border-radius: 5px; font-weight: bold;"">{voucher}</span>
                </p>
                <p style=""color: #8d4e00; font-size: 14px; margin: 0; font-style: italic;"">
                    *Áp dụng cho đơn hàng từ 200.000đ. Có hiệu lực trong 7 ngày.
                </p>
            </div>

            <a href=""{BaseUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #ff6b35, #ff8c42); color: white; padding: 15px 40px; text-decoration: none; border-radius: 30px; font-size: 18px; font-weight: bold; margin: 20px 0; box-shadow: 0 4px 15px rgba(255, 107, 53, 0.3); transition: transform 0.3s ease;"">
                🛒 ĐẶT HÀNG NGAY
            </a>
        </div>

        <div style=""background-color: #333; color: white; padding: 25px 30px; text-align: center;"">
            <p style=""margin: 0 0 0 0; font-size: 16px; font-weight: bold;"">
                📞 Hotline: {Hotline} | 🌐 {UrlDisplay}
            </p>
        </div>

        <div style=""background-color: #222; color: #999; padding: 20px 30px; text-align: center; font-size: 12px; line-height: 1.5;"">
            <p style=""margin: 0 0 10px 0;"">
                Bạn nhận được email này vì đã đăng ký tài khoản tại FastFood Express.
            </p>
            <p style=""margin: 0;"">
                © {DateTime.Now.Year} {Trademark}. Bảo lưu mọi quyền. | 
                <a href=""{BaseUrl}/privacy-policy"" style=""color: #ff6b35; text-decoration: none;"">Chính sách bảo mật</a>
            </p>
        </div>
    </div>
</div>
";
        }

        public static string VerifyAccount(string token)
        {
            return $@"
<div style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f5f5f5;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; box-shadow: 0 4px 8px rgba(0,0,0,0.1);"">
        
        <div style=""background: linear-gradient(135deg, #ff6b35, #ff8c42); padding: 40px 20px; text-align: center; color: white;"">
            <h1 style=""margin: 0; font-size: 32px; font-weight: bold; text-shadow: 2px 2px 4px rgba(0,0,0,0.3);"">
                🍔 {Trademark}
            </h1>
            <p style=""margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;"">
                {Slogan}
            </p>
        </div>

        <div style=""padding: 40px 30px; text-align: center;"">
            <h2 style=""color: #333; font-size: 28px; margin: 0 0 20px 0; line-height: 1.3;"">
                ✉️ Xác minh địa chỉ email của bạn
            </h2>
            
            <p style=""color: #666; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;"">
                Chào bạn! Cảm ơn bạn đã đăng ký tài khoản tại {Trademark}. Để hoàn tất quá trình đăng ký và bảo mật tài khoản, vui lòng xác minh địa chỉ email của bạn.
            </p>

            <div style=""background: linear-gradient(135deg, #e8f5e8, #c8e6c9); border: 2px solid #4caf50; border-radius: 15px; padding: 25px; margin: 30px 0;"">
                <div style=""background-color: #4caf50; color: white; padding: 5px 15px; border-radius: 20px; display: inline-block; font-size: 12px; font-weight: bold; margin-bottom: 15px;"">
                    🔒 BẢO MẬT TÀI KHOẢN
                </div>
                <h3 style=""color: #2e7d32; margin: 0 0 15px 0; font-size: 20px;"">
                    Chỉ cần 1 cú click để xác minh!
                </h3>
                <p style=""color: #388e3c; font-size: 14px; margin: 0; line-height: 1.5;"">
                    Sau khi xác minh, bạn sẽ có thể sử dụng đầy đủ các tính năng và nhận được các ưu đãi độc quyền từ chúng tôi.
                </p>
            </div>

            <a href=""{BaseUrl}/verify-account?token={token}"" style=""display: inline-block; background: linear-gradient(135deg, #4caf50, #66bb6a); color: white; padding: 18px 45px; text-decoration: none; border-radius: 30px; font-size: 18px; font-weight: bold; margin: 20px 0; box-shadow: 0 4px 15px rgba(76, 175, 80, 0.3);"">
                ✅ XÁC MINH EMAIL NGAY
            </a>

            <div style=""background-color: #fff8e1; border-left: 4px solid #ffc107; padding: 20px; margin: 30px 0; text-align: left;"">
                <p style=""color: #f57f17; font-size: 14px; margin: 0 0 10px 0; font-weight: bold;"">
                    ⚠️ Lưu ý quan trọng:
                </p>
                <p style=""color: #f9a825; font-size: 14px; margin: 0; line-height: 1.5;"">
                    • Link xác minh có hiệu lực trong 24 giờ<br>
                    • Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email<br>
                    • Liên hệ hotline nếu cần hỗ trợ: {Hotline}
                </p>
            </div>
        </div>

        <div style=""background-color: #333; color: white; padding: 25px 30px; text-align: center;"">
            <p style=""margin: 0 0 0 0; font-size: 16px; font-weight: bold;"">
                📞 Hotline: {Hotline} | 🌐 {UrlDisplay}
            </p>
        </div>

        <div style=""background-color: #222; color: #999; padding: 20px 30px; text-align: center; font-size: 12px; line-height: 1.5;"">
            <p style=""margin: 0 0 10px 0;"">
                Bạn nhận được email này vì đã đăng ký tài khoản tại {Trademark}.
            </p>
            <p style=""margin: 0;"">
                © {DateTime.Now.Year} {Trademark} {Trademark}. Bảo lưu mọi quyền. | 
                <a href=""{BaseUrl}/privacy-policy"" style=""color: #ff6b35; text-decoration: none;"">Chính sách bảo mật</a>
            </p>
        </div>
    </div>
</div>
";
        }

        public static string ForgotPassword(string toekn)
        {
            return $@"
<div style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f5f5f5;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; box-shadow: 0 4px 8px rgba(0,0,0,0.1);"">
        
        <div style=""background: linear-gradient(135deg, #ff6b35, #ff8c42); padding: 40px 20px; text-align: center; color: white;"">
            <h1 style=""margin: 0; font-size: 32px; font-weight: bold; text-shadow: 2px 2px 4px rgba(0,0,0,0.3);"">
                🍔 {Trademark}
            </h1>
            <p style=""margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;"">
                {Slogan}
            </p>
        </div>

        <div style=""padding: 40px 30px; text-align: center;"">
            <h2 style=""color: #333; font-size: 28px; margin: 0 0 20px 0; line-height: 1.3;"">
                🔑 Yêu cầu đặt lại mật khẩu
            </h2>
            
            <p style=""color: #666; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;"">
                Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản {Trademark} của bạn. Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.
            </p>

            <div style=""background: linear-gradient(135deg, #fff3e0, #ffe0b2); border: 2px solid #ff8c42; border-radius: 15px; padding: 25px; margin: 30px 0; position: relative;"">
                <div style=""background-color: #ff6b35; color: white; padding: 5px 15px; border-radius: 20px; display: inline-block; font-size: 12px; font-weight: bold; margin-bottom: 15px;"">
                    🔐 BẢO MẬT
                </div>
                <h3 style=""color: #d84315; margin: 0 0 15px 0; font-size: 20px;"">
                    Nhấn vào nút bên dưới để đặt lại mật khẩu
                </h3>
                <p style=""color: #8d4e00; font-size: 14px; margin: 0; font-style: italic;"">
                    *Liên kết này sẽ hết hiệu lực sau 24 giờ vì lý do bảo mật.
                </p>
            </div>

            <a href=""{BaseUrl}/reset-password?token={toekn}"" style=""display: inline-block; background: linear-gradient(135deg, #ff6b35, #ff8c42); color: white; padding: 15px 40px; text-decoration: none; border-radius: 30px; font-size: 18px; font-weight: bold; margin: 20px 0; box-shadow: 0 4px 15px rgba(255, 107, 53, 0.3);"">
                🔑 ĐẶT LẠI MẬT KHẨU
            </a>

            <p style=""color: #999; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;"">
                Nếu nút bên trên không hoạt động, vui lòng sao chép và dán liên kết sau vào trình duyệt:<br>
                <span style=""background-color: #f5f5f5; padding: 10px; border-radius: 5px; display: inline-block; margin-top: 10px; word-break: break-all; font-size: 12px; color: #666;"">
                    {BaseUrl}/reset-password?token={toekn}
                </span>
            </p>
        </div>

        <div style=""background-color: #333; color: white; padding: 25px 30px; text-align: center;"">
            <p style=""margin: 0 0 0 0; font-size: 16px; font-weight: bold;"">
                📞 Hotline: {Hotline} | 🌐 {UrlDisplay}
            </p>
        </div>

        <div style=""background-color: #222; color: #999; padding: 20px 30px; text-align: center; font-size: 12px; line-height: 1.5;"">
            <p style=""margin: 0 0 10px 0;"">
                Bạn nhận được email này vì có yêu cầu đặt lại mật khẩu cho tài khoản tại {Trademark}.
            </p>
            <p style=""margin: 0;"">
                © {DateTime.Now.Year} {Trademark}. Bảo lưu mọi quyền. | 
                <a href=""{BaseUrl}/privacy-policy"" style=""color: #ff6b35; text-decoration: none;"">Chính sách bảo mật</a>
            </p>
        </div>
    </div>
</div>
";
        }
    }
}
