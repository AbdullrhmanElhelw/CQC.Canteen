using System.ComponentModel.DataAnnotations;

namespace CQC.Canteen.Domain.Entities.Common;


public abstract class Entity<TKey> : IEntity<TKey>
{
    [Key]
    public TKey Id { get; set; }
}
