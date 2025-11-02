using CQC.Canteen.UI.Commands;
using CQC.Canteen.UI.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;

        private BaseViewModel? _currentAdminPageViewModel;
        public BaseViewModel? CurrentAdminPageViewModel
        {
            get => _currentAdminPageViewModel;
            set => SetProperty(ref _currentAdminPageViewModel, value);
        }

        private bool _isProductsActive;
        public bool IsProductsActive
        {
            get => _isProductsActive;
            set => SetProperty(ref _isProductsActive, value);
        }

        private bool _isCustomersActive;
        public bool IsCustomersActive
        {
            get => _isCustomersActive;
            set => SetProperty(ref _isCustomersActive, value);
        }

        private bool _isCategoriesActive;
        public bool IsCategoriesActive
        {
            get => _isCategoriesActive;
            set => SetProperty(ref _isCategoriesActive, value);
        }

        public ICommand ShowProductsCommand { get; }
        public ICommand ShowCategoriesCommand { get; }
        public ICommand ShowCustomersCommand { get; }

        public AdminDashboardViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            ShowProductsCommand = new RelayCommand<object>(_ => ExecuteShowProducts());
            ShowCategoriesCommand = new RelayCommand<object>(_ => ExecuteShowCategories());
            ShowCustomersCommand = new RelayCommand<object>(_ => ExecuteShowCustomers());

            ExecuteShowProducts(); // الافتراضي عند التشغيل
        }

        private void ResetActiveStates()
        {
            IsProductsActive = false;
            IsCustomersActive = false;
            IsCategoriesActive = false;
        }

        private void ExecuteShowProducts()
        {
            ResetActiveStates();
            IsProductsActive = true;
            CurrentAdminPageViewModel = _serviceProvider.GetRequiredService<ProductManagementViewModel>();
        }

        private void ExecuteShowCategories()
        {
            ResetActiveStates();
            IsCategoriesActive = true;
            CurrentAdminPageViewModel = _serviceProvider.GetRequiredService<CategoryManagementViewModel>();
        }

        private void ExecuteShowCustomers()
        {
            ResetActiveStates();
            IsCustomersActive = true;
            CurrentAdminPageViewModel = _serviceProvider.GetRequiredService<CustomerManagementViewModel>();
        }
    }
}
