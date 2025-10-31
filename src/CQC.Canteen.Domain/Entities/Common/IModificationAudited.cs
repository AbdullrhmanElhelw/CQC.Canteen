namespace CQC.Canteen.Domain.Entities.Common;

public interface IModificationAudited
{
    DateTime? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}
