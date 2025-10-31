using FluentValidation;

namespace CQC.Canteen.BusinessLogic.DTOs.Products;

public class ProductDetailsDtoValidator : AbstractValidator<ProductDetailsDto>
{
    public ProductDetailsDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف الصنف غير صحيح.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الصنف مطلوب.")
            .MaximumLength(100).WithMessage("الاسم طويل جداً.");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("سعر البيع لازم يكون أكبر من صفر.");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("سعر الشراء لا يمكن أن يكون سالب.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("الكمية لا يمكن أن تكون سالبة.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("يجب اختيار فئة.");
    }
}