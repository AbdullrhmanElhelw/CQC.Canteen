using CQC.Canteen.UI.Commands;
using CQC.Canteen.UI.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;

        private BaseViewModel _currentAdminPageViewModel;
        public BaseViewModel CurrentAdminPageViewModel
        {
            get => _currentAdminPageViewModel;
            set => SetProperty(ref _currentAdminPageViewModel, value);
        }

        public ICommand ShowProductsCommand { get; }
        public ICommand ShowCategoriesCommand { get; }
        public ICommand ShowCustomersCommand { get; } // 👈 الجديد

        public AdminDashboardViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            ShowProductsCommand = new RelayCommand<object>(
                (p) => ExecuteShowProducts()
            );

            ShowCategoriesCommand = new RelayCommand<object>(
                (p) => ExecuteShowCategories()
            );

            ShowCustomersCommand = new RelayCommand<object>(
                (p) => ExecuteShowCustomers()
            );

            ExecuteShowProducts();
        }

        private void ExecuteShowProducts()
        {
            CurrentAdminPageViewModel = _serviceProvider.GetRequiredService<ProductManagementViewModel>();
        }

        private void ExecuteShowCategories()
        {
            CurrentAdminPageViewModel = _serviceProvider.GetRequiredService<CategoryManagementViewModel>();
        }

        private void ExecuteShowCustomers()
        {
            CurrentAdminPageViewModel = _serviceProvider.GetRequiredService<CustomerManagementViewModel>();
        }
    }
}
