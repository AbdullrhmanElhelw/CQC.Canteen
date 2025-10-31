using CQC.Canteen.Domain.Entities.Common;
using CQC.Canteen.Domain.Enums;

namespace CQC.Canteen.Domain.Entities;

public sealed class User : AuditableEntity<int>
{
    public string Name { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }

    public ICollection<Order> CreatedOrders { get; set; } = [];
}
