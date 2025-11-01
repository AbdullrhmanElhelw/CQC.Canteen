using FluentValidation;

namespace CQC.Canteen.BusinessLogic.DTOs.Customers;

public class CustomerDetailsValidator : AbstractValidator<CustomerDetailsDto>
{
    public CustomerDetailsValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم العميل مطلوب.")
            .MaximumLength(100).WithMessage("اسم العميل لا يجب أن يتجاوز 100 حرف.");

        RuleFor(x => x.CurrentBalance)
            .GreaterThanOrEqualTo(0).WithMessage("الرصيد الحالي لا يمكن أن يكون سالبًا.");

        When(x => x.IsMilitary, () =>
        {
            RuleFor(x => x.Rank)
                .NotNull().WithMessage("يجب تحديد الرتبة للعميل العسكري.");
        });
    }
}