using CQC.Canteen.BusinessLogic.DTOs.Authentication;
using FluentResults;

namespace CQC.Canteen.BusinessLogic.Services.Authentication;

public interface IAuthenticationService
{
    Task<Result<UserLoginResultDto>> LoginAsync(LoginDto loginDto, CancellationToken token);
}