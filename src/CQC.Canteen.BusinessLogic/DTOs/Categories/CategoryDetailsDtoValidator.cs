using FluentValidation;

namespace CQC.Canteen.BusinessLogic.DTOs.Categories;

// CategoryDetailsDtoValidator.cs
public class CategoryDetailsDtoValidator : AbstractValidator<CategoryDetailsDto>
{
    public CategoryDetailsDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرّف الفئة غير صالح.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الفئة مطلوب.")
            .MaximumLength(100).WithMessage("اسم الفئة لا يمكن أن يتجاوز 100 حرف.");
    }
}