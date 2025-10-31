using CQC.Canteen.Domain.Entities.Common;
using CQC.Canteen.Domain.Enums;

namespace CQC.Canteen.Domain.Entities;

public sealed class Order : AuditableEntity<int>
{
    public decimal TotalAmount { get; set; }
    public PaymentMethod paymentMethod { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; }

    public int? CustomerId { get; set; }
    public Customer Customer { get; set; }

    public ICollection<OrderDetails> OrderDetails { get; set; } = [];
}
