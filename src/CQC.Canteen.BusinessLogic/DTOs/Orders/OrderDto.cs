using CQC.Canteen.Domain.Enums;

namespace CQC.Canteen.BusinessLogic.DTOs.Orders;

public sealed class OrderDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public int CreatedByUserId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
}