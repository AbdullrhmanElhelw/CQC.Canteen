using CQC.Canteen.BusinessLogic.DTOs.Customers;
using FluentResults;

namespace CQC.Canteen.BusinessLogic.Services.Customers;

public interface ICustomerService
{
    Task<Result<List<CustomerDto>>> GetAllCustomersAsync(CancellationToken token);
    Task<Result<CustomerDto>> AddCustomerAsync(CreateCustomerDto dto, CancellationToken token);
    Task<Result<CustomerDetailsDto>> GetCustomerDetailsByIdAsync(int id, CancellationToken token);
    Task<Result<CustomerDto>> UpdateCustomerAsync(CustomerDetailsDto dto, CancellationToken token);
    Task<Result> SettleCustomerBalanceAsync(int id, CancellationToken token);
}
