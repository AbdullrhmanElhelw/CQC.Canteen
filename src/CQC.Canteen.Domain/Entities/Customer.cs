using CQC.Canteen.Domain.Entities.Common;

namespace CQC.Canteen.Domain.Entities;

public sealed class Customer : AuditableEntity<int>
{
    public string Name { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;


    public ICollection<Order> Orders { get; set; } = [];
}
