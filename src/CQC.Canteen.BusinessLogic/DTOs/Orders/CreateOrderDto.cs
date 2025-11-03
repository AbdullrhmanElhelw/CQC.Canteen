using CQC.Canteen.Domain.Enums;

namespace CQC.Canteen.BusinessLogic.DTOs.Orders;

public record CreateOrderDto
{
    public int CreatedByUserId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public int? CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
    public decimal AmountPaid { get; set; }
}
