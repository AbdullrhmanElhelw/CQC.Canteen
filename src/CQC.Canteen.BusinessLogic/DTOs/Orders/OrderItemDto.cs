namespace CQC.Canteen.BusinessLogic.DTOs.Orders;

public sealed class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
