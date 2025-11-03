using CQC.Canteen.UI.Commands; // <-- اتأكد إن دي موجودة
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input; // <-- اتأكد إن دي موجودة (عشان الـ MessageBox)

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

        // *** تم التعديل هنا: إضافة الـ Command الجديد ***
        public ICommand LogoutCommand { get; }

        public MainViewModel(IServiceProvider serviceProvider, LoginViewModel loginViewModel)
        {
            _serviceProvider = serviceProvider;

            loginViewModel.LoginSucceeded += OnLoginSucceeded;
            _currentViewModel = loginViewModel;
            IsLoggedIn = false;

            // *** تم التعديل هنا: تهيئة الـ Command ***
            // (استخدمت object كنوع عام للـ T)
            LogoutCommand = new RelayCommand<object>(ExecuteLogout);
        }

        private void OnLoginSucceeded(string role)
        {
            // (اللوجيك بتاعك زي ما هو 100%)
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
                CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                (CurrentViewModel as LoginViewModel).ErrorMessage = "صلاحية المستخدم غير معروفة.";
                return;
            }

            IsLoggedIn = true;
        }

        // *** تم التعديل هنا: إضافة الميثود الخاصة بالـ Logout ***
        private void ExecuteLogout(object? _ = null)
        {
            // 1. نتأكد من المستخدم
            var result = MessageBox.Show("هل أنت متأكد أنك تريد تسجيل الخروج؟",
                                         "تأكيد تسجيل الخروج",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            // 2. أهم خطوة: نرجع البرنامج لحالة "قبل اللوجن"
            IsLoggedIn = false;

            // 3. نجيب شاشة لوجن جديدة
            var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();

            // 4. !! خطوة حرجة جداً !!
            // لازم نشترك في الحدث بتاعها تاني، عشان اللوجن الجديد يشتغل
            loginViewModel.LoginSucceeded += OnLoginSucceeded;

            // 5. نعرض شاشة اللوجن
            CurrentViewModel = loginViewModel;
        }
    }
}