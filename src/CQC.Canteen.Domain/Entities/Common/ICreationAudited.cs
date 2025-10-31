namespace CQC.Canteen.Domain.Entities.Common;

public interface ICreationAudited
{
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
}
