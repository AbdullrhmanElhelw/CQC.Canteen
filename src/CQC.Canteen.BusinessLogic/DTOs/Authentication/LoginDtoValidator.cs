using FluentValidation;

namespace CQC.Canteen.BusinessLogic.DTOs.Authentication;

public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
            .MinimumLength(3).WithMessage("اسم المستخدم يجب أن يكون على الأقل 3 أحرف.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
            .MinimumLength(4).WithMessage("كلمة المرور يجب أن تكون على الأقل 4 أحرف.");
    }
}
