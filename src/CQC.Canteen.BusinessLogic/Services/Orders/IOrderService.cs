using CQC.Canteen.BusinessLogic.DTOs.Orders;
using FluentResults;

namespace CQC.Canteen.BusinessLogic.Services.Orders;

public interface IOrderService
{
    Task<Result<OrderDto>> CreateCashOrderAsync(CreateOrderDto dto, CancellationToken token);
    Task<Result<OrderDto>> CreateDeferredOrderAsync(CreateOrderDto dto, CancellationToken token);
}
