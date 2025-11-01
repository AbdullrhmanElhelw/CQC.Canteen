using CQC.Canteen.Domain.Enums;

namespace CQC.Canteen.BusinessLogic.DTOs.Customers;

public class CreateCustomerDto
{
    public string Name { get; set; }
    public bool IsMilitary { get; set; }
    public MilitaryRank? Rank { get; set; }
}