using CQC.Canteen.Domain.Entities.Common;

namespace CQC.Canteen.Domain.Entities;

public sealed class Product : AuditableEntity<int>
{
    public string Name { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public ICollection<OrderDetails> OrderDetails { get; set; } = [];
}
