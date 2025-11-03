using CQC.Canteen.UI.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CQC.Canteen.UI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DispatcherTimer _timer;

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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _currentUserName = "مستخدم النظام";
        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        private string _currentUserRole = "Admin";
        public string CurrentUserRole
        {
            get => _currentUserRole;
            set => SetProperty(ref _currentUserRole, value);
        }

        private string _currentDate;
        public string CurrentDate
        {
            get => _currentDate;
            set => SetProperty(ref _currentDate, value);
        }

        private string _currentTime;
        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        private string _lastActivity = "آخر نشاط: لا يوجد";
        public string LastActivity
        {
            get => _lastActivity;
            set => SetProperty(ref _lastActivity, value);
        }

        public ICommand LogoutCommand { get; }

        public MainViewModel(IServiceProvider serviceProvider, LoginViewModel loginViewModel)
        {
            _serviceProvider = serviceProvider;

            // Subscribe to login event
            loginViewModel.LoginSucceeded += OnLoginSucceeded;

            _currentViewModel = loginViewModel;
            IsLoggedIn = false;

            // Initialize logout command
            LogoutCommand = new RelayCommand<object>(ExecuteLogout);

            // Setup timer for date/time updates
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Initial update
            UpdateDateTime();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            var now = DateTime.Now;

            // Arabic date format
            var culture = new CultureInfo("ar-EG");
            CurrentDate = now.ToString("dddd، d MMMM yyyy", culture);

            // Time in 12-hour format
            CurrentTime = now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture);
        }

        private void OnLoginSucceeded(string role)
        {
            // Show loading
            IsLoading = true;

            try
            {
                // Set user role for display
                CurrentUserRole = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                    ? "مدير النظام"
                    : "كاشير";

                // Navigate based on role
                if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    CurrentViewModel = _serviceProvider.GetRequiredService<AdminDashboardViewModel>();
                    LastActivity = $"تسجيل دخول مدير - {DateTime.Now:hh:mm tt}";
                }
                else if (role.Equals("Casher", StringComparison.OrdinalIgnoreCase))
                {
                    CurrentViewModel = _serviceProvider.GetRequiredService<CasherSalesViewModel>();
                    LastActivity = $"تسجيل دخول كاشير - {DateTime.Now:hh:mm tt}";
                }
                else
                {
                    CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                    (CurrentViewModel as LoginViewModel).ErrorMessage = "صلاحية المستخدم غير معروفة.";
                    IsLoading = false;
                    return;
                }

                IsLoggedIn = true;
            }
            finally
            {
                // Hide loading after a short delay for smooth transition
                var hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                hideTimer.Tick += (s, e) =>
                {
                    IsLoading = false;
                    hideTimer.Stop();
                };
                hideTimer.Start();
            }
        }

        private void ExecuteLogout(object? _ = null)
        {
            // Confirm logout
            var result = MessageBox.Show(
                "هل أنت متأكد أنك تريد تسجيل الخروج؟",
                "تأكيد تسجيل الخروج",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            // Show loading
            IsLoading = true;

            try
            {
                // Update last activity
                LastActivity = $"تسجيل خروج - {DateTime.Now:hh:mm tt}";

                // Reset login state
                IsLoggedIn = false;

                // Reset user info
                CurrentUserName = "مستخدم النظام";
                CurrentUserRole = "Admin";

                // Get new login view model
                var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();

                // Re-subscribe to login event
                loginViewModel.LoginSucceeded += OnLoginSucceeded;

                // Navigate to login
                CurrentViewModel = loginViewModel;
            }
            finally
            {
                // Hide loading
                var hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                hideTimer.Tick += (s, e) =>
                {
                    IsLoading = false;
                    hideTimer.Stop();
                };
                hideTimer.Start();
            }
        }
    }
}