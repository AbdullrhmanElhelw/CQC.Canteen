using Microsoft.Extensions.DependencyInjection;

namespace CQC.Canteen.UI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        // (1) هيستقبل الـ LoginViewModel الجاهز
        public MainViewModel(IServiceProvider serviceProvider, LoginViewModel loginViewModel)
        {
            _serviceProvider = serviceProvider;

            // (2) هيشترك في الحدث بتاعه
            loginViewModel.LoginSucceeded += OnLoginSucceeded;

            // (3) هيبدأ بشاشة اللوجن
            _currentViewModel = loginViewModel;
            IsLoggedIn = false;
        }

        // (4) دي الميثود اللي هتشتغل لما اللوجن ينجح
        private void OnLoginSucceeded(string role)
        {
            // أهم سطرين: غير الشاشة وقول إننا عملنا لوجن
            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                CurrentViewModel = _serviceProvider.GetRequiredService<AdminDashboardViewModel>();
            }
            else if (role.Equals("Casher", StringComparison.OrdinalIgnoreCase))
            {
                CurrentViewModel = _serviceProvider.GetRequiredService<CasherSalesViewModel>();
            }
            else
            {
                // (احتياطي لو الـ Role رجع حاجة غريبة)
                // ممكن نعرض شاشة خطأ، أو نرجع للوجن تاني
                CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                (CurrentViewModel as LoginViewModel).ErrorMessage = "صلاحية المستخدم غير معروفة.";
                return; // متكملش
            }

            IsLoggedIn = true;
        }
    }
}

