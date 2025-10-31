using CQC.Canteen.BusinessLogic.DTOs.Authentication;
using CQC.Canteen.Domain.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace CQC.Canteen.BusinessLogic.Services.Authentication;

public class AuthenticationService(CanteenDbContext context) : IAuthenticationService
{
    private readonly LoginDtoValidator _validator = new();

    public async Task<Result<UserLoginResultDto>> LoginAsync(LoginDto loginDto, CancellationToken token)
    {
        var validation = await _validator.ValidateAsync(loginDto, token);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(string.Join(Environment.NewLine, errors));
        }

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == loginDto.UserName, token);

        if (user is null)
            return Result.Fail("اسم المستخدم غير موجود.");

        if (user.Password != loginDto.Password)
            return Result.Fail("كلمة المرور غير صحيحة.");

        var resultDto = new UserLoginResultDto(
            Message: "تم تسجيل الدخول بنجاح.",
            Role: user.Role.ToString()
        );

        return Result.Ok(resultDto);
    }
}
