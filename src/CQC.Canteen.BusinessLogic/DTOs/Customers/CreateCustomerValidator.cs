using FluentValidation;

namespace CQC.Canteen.BusinessLogic.DTOs.Customers;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم العميل مطلوب.")
            .MaximumLength(100).WithMessage("اسم العميل لا يجب أن يتجاوز 100 حرف.");

        RuleFor(x => x.IsMilitary)
            .NotNull().WithMessage("يجب تحديد نوع العميل (عسكري أو مدني).");

        // لو عسكري لازم يحدد رتبة
        When(x => x.IsMilitary, () =>
        {
            RuleFor(x => x.Rank)
                .NotNull().WithMessage("يجب اختيار الرتبة العسكرية.");
        });
    }
}