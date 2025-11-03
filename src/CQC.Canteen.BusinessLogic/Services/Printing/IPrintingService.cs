namespace CQC.Canteen.BusinessLogic.Services.Printing;

public interface IPrintingService
{
    Task PrintReceiptAsync(IEnumerable<SaleItemDto> items, decimal totalAmount, decimal amountReceived, decimal change);
}

public class SaleItemDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = ""; // (قيمة افتراضية لتجنب الـ null)
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    // (اختياري) يمكن حساب الإجمالي في الـ DTO أيضاً
    public decimal Total => Quantity * Price;
}