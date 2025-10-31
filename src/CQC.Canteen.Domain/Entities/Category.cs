using CQC.Canteen.Domain.Entities.Common;

namespace CQC.Canteen.Domain.Entities;

public sealed class Category : AuditableEntity<int>
{
    public string Name { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
