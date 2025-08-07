using Assignment.Enum;
using Assignment.Models;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Path = System.IO.Path;

namespace Assignment.Service
{
    public class BillService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _billDirectory = "wwwroot/bills";
        private readonly string _baseApiUrl = "http://localhost";
        private readonly string _companyName = "FastFood Express";
        private readonly string _companySlogan = "Đồ ăn nhanh - Giao hàng siêu tốc!";
        private readonly string _hotlineNumber = "0999999999";
        private readonly string _websiteUrl = "www.localhost";
        private readonly string _contactEmail = "support@fastfoodexpress.com";

        public BillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GenerateBill(long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return;
            }

            string fileName = $"bill_{orderId}.pdf";
            string filePath = Path.Combine(_billDirectory, fileName);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4);

            document.SetMargins(40, 40, 40, 40);

            var robotoRegular = PdfFontFactory.CreateFont("wwwroot/fonts/Roboto-Regular.ttf", PdfEncodings.IDENTITY_H);
            var robotoBold = PdfFontFactory.CreateFont("wwwroot/fonts/Roboto-Bold.ttf", PdfEncodings.IDENTITY_H);

            var primaryColor = new DeviceRgb(255, 107, 53);
            var secondaryColor = new DeviceRgb(51, 51, 51);
            var accentColor = new DeviceRgb(76, 185, 68);
            var lightGray = new DeviceRgb(245, 245, 245);

            var header = new Table(UnitValue.CreatePercentArray(2)).SetWidth(UnitValue.CreatePercentValue(100));

            var leftHeader = new Cell()
                .Add(new Paragraph(_companyName)
                    .SetFont(robotoBold)
                    .SetFontSize(18)
                    .SetFontColor(primaryColor)
                    .SetMargin(0))
                .Add(new Paragraph(_companySlogan)
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetMargin(0))
                .SetBorder(null)
                .SetPadding(0);

            var rightHeader = new Cell()
                .Add(new Paragraph("HÓA ĐƠN")
                    .SetFont(robotoBold)
                    .SetFontSize(24)
                    .SetFontColor(primaryColor)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMargin(0))
                .Add(new Paragraph($"MÃ ĐƠN: {orderId.ToString().Substring(0, Math.Min(8, orderId.ToString().Length)).ToUpper()}")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMargin(0))
                .Add(new Paragraph($"NGÀY: {order.CreatedTime:dd/MM/yyyy HH:mm}")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMargin(0))
                .SetBorder(null)
                .SetPadding(0);

            header.AddCell(leftHeader);
            header.AddCell(rightHeader);
            document.Add(header);

            document.Add(new LineSeparator(new SolidLine(1f)).SetStrokeColor(primaryColor).SetMarginTop(10).SetMarginBottom(10));

            var infoTable = new Table(UnitValue.CreatePercentArray(2)).SetWidth(UnitValue.CreatePercentValue(100));

            var customerInfo = new Cell()
                .Add(new Paragraph("THÔNG TIN KHÁCH HÀNG")
                    .SetFont(robotoBold)
                    .SetFontSize(14)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(5))
                .Add(new Paragraph($"Tên: {order.Name}")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(2))
                .Add(new Paragraph($"SĐT: {order.Phone}")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(2))
                .SetBorder(null)
                .SetPadding(10)
                .SetBackgroundColor(lightGray);

            var orderInfo = new Cell()
                .Add(new Paragraph("THÔNG TIN ĐƠN HÀNG")
                    .SetFont(robotoBold)
                    .SetFontSize(14)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(5))
                .Add(new Paragraph("Loại: Giao hàng")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(2))
                .Add(new Paragraph($"Địa chỉ: {order.Address}")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(2))
                .SetBorder(null)
                .SetPadding(10);

            infoTable.AddCell(customerInfo);
            infoTable.AddCell(orderInfo);
            document.Add(infoTable);

            document.Add(new Paragraph("").SetMarginBottom(10));

            var orderTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 45, 20, 20 }))
                .SetWidth(UnitValue.CreatePercentValue(100));

            var headerCells = new string[] { "SỐ LƯỢNG", "MÓN ĂN", "ĐƠN GIÁ", "THÀNH TIỀN" };
            foreach (var headerText in headerCells)
            {
                orderTable.AddHeaderCell(new Cell()
                    .Add(new Paragraph(headerText)
                        .SetFont(robotoBold)
                        .SetFontSize(10)
                        .SetFontColor(DeviceRgb.WHITE))
                    .SetBackgroundColor(primaryColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(8));
            }

            bool isEvenRow = false;
            foreach (var item in order.OrderDetails)
            {
                var rowColor = isEvenRow ? lightGray : DeviceRgb.WHITE;

                orderTable.AddCell(new Cell()
                    .Add(new Paragraph(item.TotalQuantityPreItems.ToString())
                        .SetFont(robotoRegular)
                        .SetFontSize(10)
                        .SetFontColor(secondaryColor))
                    .SetBackgroundColor(rowColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(8)
                    .SetBorder(null));

                string productName = item.Product?.Name ?? item.ProductId.ToString();
                orderTable.AddCell(new Cell()
                    .Add(new Paragraph(productName)
                        .SetFont(robotoRegular)
                        .SetFontSize(10)
                        .SetFontColor(secondaryColor))
                    .SetBackgroundColor(rowColor)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetPadding(8)
                    .SetBorder(null));

                double unitPrice = item.TotalQuantityPreItems > 0 ? item.PricePreItems / item.TotalQuantityPreItems : 0;
                orderTable.AddCell(new Cell()
                    .Add(new Paragraph(unitPrice.ToString("N0") + " VND")
                        .SetFont(robotoRegular)
                        .SetFontSize(10)
                        .SetFontColor(secondaryColor))
                    .SetBackgroundColor(rowColor)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetPadding(8)
                    .SetBorder(null));

                orderTable.AddCell(new Cell()
                    .Add(new Paragraph(item.TotalPricePreItems.ToString("N0") + " VND")
                        .SetFont(robotoRegular)
                        .SetFontSize(10)
                        .SetFontColor(secondaryColor))
                    .SetBackgroundColor(rowColor)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetPadding(8)
                    .SetBorder(null));

                isEvenRow = !isEvenRow;
            }

            document.Add(orderTable);

            document.Add(new Paragraph("").SetMarginBottom(10));

            var summaryTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 }))
                .SetWidth(UnitValue.CreatePercentValue(100));

            var paymentInfo = new Cell()
                .Add(new Paragraph("PHƯƠNG THỨC THANH TOÁN")
                    .SetFont(robotoBold)
                    .SetFontSize(12)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(5))
                .Add(new Paragraph("Chuyển khoản")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(2))
                .Add(new Paragraph("Trạng thái: Đã thanh toán")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(accentColor)
                    .SetMarginBottom(2))
                .SetBorder(null)
                .SetPadding(10);

            var summaryInfo = new Cell()
                .Add(new Paragraph($"Tổng số lượng: {order.TotalQuantity}")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(3)
                    .SetTextAlignment(TextAlignment.RIGHT))
                .Add(new Paragraph($"Tổng tiền hàng: {order.TotalPrice.ToString("N0")} VND")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(secondaryColor)
                    .SetMarginBottom(3)
                    .SetTextAlignment(TextAlignment.RIGHT));

            if (order.Discount > 0)
            {
                summaryInfo.Add(new Paragraph($"Giảm giá: -{order.Discount.ToString("N0")} VND")
                    .SetFont(robotoRegular)
                    .SetFontSize(10)
                    .SetFontColor(accentColor)
                    .SetMarginBottom(3)
                    .SetTextAlignment(TextAlignment.RIGHT));
            }

            summaryInfo.Add(new Paragraph($"Phí vận chuyển: {order.Fee.ToString("N0")} VND")
                .SetFont(robotoRegular)
                .SetFontSize(10)
                .SetFontColor(secondaryColor)
                .SetMarginBottom(3)
                .SetTextAlignment(TextAlignment.RIGHT))
            .Add(new Paragraph($"VAT: {order.Vat.ToString("N0")} VND")
                .SetFont(robotoRegular)
                .SetFontSize(10)
                .SetFontColor(secondaryColor)
                .SetMarginBottom(8)
                .SetTextAlignment(TextAlignment.RIGHT))
            .Add(new LineSeparator(new SolidLine(1f)).SetStrokeColor(primaryColor).SetMarginBottom(5))
            .Add(new Paragraph($"TỔNG CỘNG: {order.TotalBill.ToString("N0")} VND")
                .SetFont(robotoBold)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetTextAlignment(TextAlignment.RIGHT))
            .SetBorder(null)
            .SetPadding(10)
            .SetBackgroundColor(lightGray);

            summaryTable.AddCell(paymentInfo);
            summaryTable.AddCell(summaryInfo);
            document.Add(summaryTable);

            document.Add(new Paragraph("").SetMarginTop(20));
            document.Add(new LineSeparator(new SolidLine(1f)).SetStrokeColor(primaryColor));

            document.Add(new Paragraph("CẢM ƠN QUÝ KHÁCH ĐÃ SỬ DỤNG DỊCH VỤ!")
                .SetFont(robotoBold)
                .SetFontSize(14)
                .SetFontColor(primaryColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(10));

            document.Add(new Paragraph($"Hotline: {_hotlineNumber} | Website: {_websiteUrl} | Email: {_contactEmail}")
                .SetFont(robotoRegular)
                .SetFontSize(9)
                .SetFontColor(secondaryColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(5));

            document.Close();

            order.BillUrl = $"{_baseApiUrl}/bills/{fileName}";
            await _context.SaveChangesAsync();
        }
    }
}