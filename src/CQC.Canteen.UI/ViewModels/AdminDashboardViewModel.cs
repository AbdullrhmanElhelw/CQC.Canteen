using CQC.Canteen.UI.Commands;
using CQC.Canteen.UI.ViewModels.Pages; // <-- (Namespace جديد)
using Microsoft.Extensions.DependencyInjection; // <-- (مهم)
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;

        // "المسرح" الداخلي لصفحات الأدمن
        private BaseViewModel _currentAdminPageViewModel;
        public BaseViewModel CurrentAdminPageViewModel
        {
            get => _currentAdminPageViewModel;
            set => SetProperty(ref _currentAdminPageViewModel, value);
        }

        // الأوامر بتاعة القائمة الجانبية
        public ICommand ShowProductsCommand { get; }
        // (قريب هنضيف باقي الأوامر هنا)
        // public ICommand ShowCustomersCommand { get; }
        // public ICommand ShowCategoriesCommand { get; }

        public AdminDashboardViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // (1) تعريف الأوامر
            // استخدمنا RelayCommand<object> عشان هو ده اللي موجود عندنا حالياً
            ShowProductsCommand = new RelayCommand<object>(
                (p) => ExecuteShowProducts(),
                (p) => true // (ممكن بعدين نخلي الزرار non-clickable لو إحنا أصلاً في الصفحة دي)
            );

            // (2) تحديد الصفحة الافتراضية
            ExecuteShowProducts();
        }

        private void ExecuteShowProducts()
        {
            // اطلب الـ ViewModel من الـ DI
            CurrentAdminPageViewModel = _serviceProvider.GetRequiredService<ProductManagementViewModel>();
        }
    }
}

