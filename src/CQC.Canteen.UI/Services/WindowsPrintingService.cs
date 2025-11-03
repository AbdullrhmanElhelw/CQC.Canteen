using CQC.Canteen.BusinessLogic.Services.Printing;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows;

namespace CQC.Canteen.UI.Services
{
    public class WindowsPrintingService : IPrintingService
    {
        private List<SaleItemDto> _items;
        private decimal _totalAmount;
        private decimal _amountReceived;
        private decimal _change;

        public Task PrintReceiptAsync(IEnumerable<SaleItemDto> items, decimal totalAmount, decimal amountReceived, decimal change)
        {
            _items = items.ToList();
            _totalAmount = totalAmount;
            _amountReceived = amountReceived;
            _change = change;

            PrintDocument pd = new PrintDocument();
            // pd.PrinterSettings.PrinterName = "اسم طابعة الإيصالات";
            pd.PrintPage += PrintPageHandler;

            return Task.Run(() =>
            {
                try
                {
                    pd.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل طباعة الإيصال: {ex.Message}", "خطأ طباعة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    pd.PrintPage -= PrintPageHandler;
                }
            });
        }

        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            // (استخدم خط Monospaced مثل Courier New لضمان محاذاة الأعمدة)

            // *** (بداية التعديل لحل CS0104) ***
            // نستخدم المسار الكامل لتجنب التضارب مع System.Windows.FontStyle
            using (Font regularFont = new Font("Courier New", 10, System.Drawing.FontStyle.Regular))
            using (Font boldFont = new Font("Courier New", 10, System.Drawing.FontStyle.Bold))
            // *** (نهاية التعديل) ***

            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                float yPos = 10;
                float leftMargin = 10;
                float columnWidth = 70;

                // ... (باقي كود الرسم كما هو) ...
                g.DrawString("إيصال - كانتين CQC", boldFont, brush, leftMargin, yPos);
                yPos += 25;
                g.DrawString($"التاريخ: {DateTime.Now:yyyy/MM/dd HH:mm}", regularFont, brush, leftMargin, yPos);
                yPos += 20;
                g.DrawString("----------------------------------", regularFont, brush, leftMargin, yPos);
                yPos += 20;

                g.DrawString("الإجمالي", regularFont, brush, leftMargin, yPos);
                g.DrawString("السعر", regularFont, brush, leftMargin + columnWidth, yPos);
                g.DrawString("الكمية", regularFont, brush, leftMargin + (columnWidth * 2), yPos);
                g.DrawString("الصنف", regularFont, brush, leftMargin + (columnWidth * 3), yPos);
                yPos += 20;

                foreach (var item in _items)
                {
                    g.DrawString(item.Total.ToString("N2"), regularFont, brush, leftMargin, yPos);
                    g.DrawString(item.Price.ToString("N2"), regularFont, brush, leftMargin + columnWidth, yPos);
                    g.DrawString(item.Quantity.ToString(), regularFont, brush, leftMargin + (columnWidth * 2), yPos);
                    g.DrawString(item.Name, regularFont, brush, leftMargin + (columnWidth * 3), yPos);
                    yPos += 20;
                }

                g.DrawString("----------------------------------", regularFont, brush, leftMargin, yPos);
                yPos += 20;

                g.DrawString($"الإجمالي: {_totalAmount:N2} ج.م", boldFont, brush, leftMargin, yPos);
                yPos += 25;
                g.DrawString($"المستلم: {_amountReceived:N2} ج.م", regularFont, brush, leftMargin, yPos);
                yPos += 20;
                g.DrawString($"الباقي: {_change:N2} ج.م", regularFont, brush, leftMargin, yPos);
                yPos += 40;

                g.DrawString("شكراً لزيارتكم!", boldFont, brush, leftMargin, yPos);

                e.HasMorePages = false;
            }
        }
    }
}