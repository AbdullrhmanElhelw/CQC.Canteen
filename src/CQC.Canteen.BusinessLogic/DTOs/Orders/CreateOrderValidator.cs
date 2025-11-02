using CQC.Canteen.Domain.Enums;
using FluentValidation;

namespace CQC.Canteen.BusinessLogic.DTOs.Orders;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CreatedByUserId).GreaterThan(0);
        RuleFor(x => x.Items).NotNull().NotEmpty()
            .WithMessage("لا بد من وجود أصناف في الطلب.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0m);
        });

        When(x => x.PaymentMethod == PaymentMethod.Deferred, () =>
        {
            RuleFor(x => x.CustomerId).NotNull().WithMessage("CustomerId مطلوب للدفع الآجل.");
        });
    }
}
