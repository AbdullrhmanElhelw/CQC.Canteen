using CQC.Canteen.Domain.Entities.Common;
using CQC.Canteen.Domain.Enums;

namespace CQC.Canteen.Domain.Entities;

public sealed class Customer : AuditableEntity<int>
{
    public string Name { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsMilitary { get; set; }
    public MilitaryRank? Rank { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}
