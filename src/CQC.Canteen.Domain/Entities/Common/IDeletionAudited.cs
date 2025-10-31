namespace CQC.Canteen.Domain.Entities.Common;

public interface IDeletionAudited
{
    bool IsDeleted { get; set; }
    DateTime? DeletedOn { get; set; }
    string? DeletedBy { get; set; }
}
