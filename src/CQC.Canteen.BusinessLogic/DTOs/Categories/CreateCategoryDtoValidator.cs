using FluentValidation;

namespace CQC.Canteen.BusinessLogic.DTOs.Categories;

// CreateCategoryDtoValidator.cs
public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الفئة مطلوب.")
            .MaximumLength(100).WithMessage("اسم الفئة لا يمكن أن يتجاوز 100 حرف.");
    }
}
