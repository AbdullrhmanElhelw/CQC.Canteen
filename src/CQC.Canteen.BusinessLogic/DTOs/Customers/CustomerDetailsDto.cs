using CQC.Canteen.Domain.Enums;

namespace CQC.Canteen.BusinessLogic.DTOs.Customers;

public class CustomerDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }

    public bool IsMilitary { get; set; }
    public MilitaryRank? Rank { get; set; }
}