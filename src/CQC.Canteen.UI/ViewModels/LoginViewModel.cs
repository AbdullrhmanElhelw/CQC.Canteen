using CQC.Canteen.BusinessLogic.DTOs.Authentication;
using CQC.Canteen.BusinessLogic.Services.Authentication;
using CQC.Canteen.UI.Commands;
using System.Windows.Controls;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        // (1) هنحقن السيرفيس بتاعتك
        private readonly IAuthenticationService _authService;

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }

        // (2) الحدث اللي هيبلغ MainViewModel بالنجاح
        public event Action<string> LoginSucceeded; // هيبعت الـ Role كـ string

        public LoginViewModel(IAuthenticationService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(PasswordBox passwordBox)
        {
            return !IsLoading; // متقدرش تدوس لو بيحمل
        }

        private async void ExecuteLogin(PasswordBox passwordBox)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var loginDto = new LoginDto(this.Username, passwordBox.Password);

            try
            {
                // (3) هننادي على السيرفيس بتاعتك
                var result = await _authService.LoginAsync(loginDto, default);

                if (result.IsSuccess)
                {
                    // (4) نجحنا! اطلق الحدث
                    LoginSucceeded?.Invoke(result.Value.Role);
                }
                else
                {
                    // فشل
                    ErrorMessage = string.Join("\n", result.Errors.Select(e => e.Message));
                }
            }
            catch (Exception ex)
            {
                // فشل عام (زي لو الداتابيز واقعة)
                ErrorMessage = $"حدث خطأ فني: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

