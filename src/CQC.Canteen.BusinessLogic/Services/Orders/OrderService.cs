using CQC.Canteen.BusinessLogic.DTOs.Orders;
using CQC.Canteen.Domain.Data;
using CQC.Canteen.Domain.Entities;
using CQC.Canteen.Domain.Enums;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CQC.Canteen.BusinessLogic.Services.Orders;

public class OrderService(CanteenDbContext context, IValidator<CreateOrderDto> createValidator) : IOrderService
{
    public Task<Result<OrderDto>> CreateCashOrderAsync(CreateOrderDto dto, CancellationToken token)
        => CreateOrderCoreAsync(dto with { PaymentMethod = PaymentMethod.Cash }, token);

    public Task<Result<OrderDto>> CreateDeferredOrderAsync(CreateOrderDto dto, CancellationToken token)
        => CreateOrderCoreAsync(dto with { PaymentMethod = PaymentMethod.Deferred }, token);

    private async Task<Result<OrderDto>> CreateOrderCoreAsync(CreateOrderDto dto, CancellationToken token)
    {

        var val = await createValidator.ValidateAsync(dto, token);
        if (!val.IsValid)
            return Result.Fail(val.Errors.Select(e => e.ErrorMessage));

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, token);

        foreach (var item in dto.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var prod))
                return Result.Fail($"الصنف رقم {item.ProductId} غير موجود.");

            if (!prod.IsActive)
                return Result.Fail($"الصنف {prod.Name} غير نشط.");

            if (prod.StockQuantity < item.Quantity)
                return Result.Fail($"الكمية غير متاحة للصنف {prod.Name}. المتاح: {prod.StockQuantity}");
        }

        // 3) Calc total
        var total = dto.Items.Sum(i => i.UnitPrice * i.Quantity);

        // 4) If deferred, must have customer & be active
        Customer? customer = null;
        if (dto.PaymentMethod == PaymentMethod.Deferred)
        {
            if (dto.CustomerId is null)
                return Result.Fail("CustomerId مطلوب للدفع الآجل.");

            customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId, token);
            if (customer is null)
                return Result.Fail("العميل غير موجود.");

            if (!customer.IsActive)
                return Result.Fail("العميل غير نشط.");
        }

        // 5) Create order
        var order = new Order
        {
            CreatedByUserId = dto.CreatedByUserId,
            paymentMethod = dto.PaymentMethod,
            CustomerId = dto.CustomerId,
            TotalAmount = total
        };

        foreach (var item in dto.Items)
        {
            order.OrderDetails.Add(new OrderDetails
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        // 6) Update stock
        foreach (var item in dto.Items)
        {
            var prod = products[item.ProductId];
            prod.StockQuantity -= item.Quantity;
        }

        // 7) If deferred, update customer balance
        if (customer is not null)
        {
            customer.CurrentBalance += total;
        }

        // 8) Save
        await context.Orders.AddAsync(order, token);
        await context.SaveChangesAsync(token);

        // 9) Return dto
        var result = new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.paymentMethod,
            CreatedByUserId = order.CreatedByUserId,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt
        };

        return Result.Ok(result);
    }
}