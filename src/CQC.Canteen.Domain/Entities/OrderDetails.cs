using CQC.Canteen.Domain.Entities.Common;

namespace CQC.Canteen.Domain.Entities;

public sealed class OrderDetails : AuditableEntity<int>
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; }
}
