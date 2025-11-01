using CQC.Canteen.BusinessLogic.DTOs.Customers;
using CQC.Canteen.BusinessLogic.Services.Customers;
using CQC.Canteen.UI.Commands;
using CQC.Canteen.UI.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages
{
    public class CustomerManagementViewModel : BaseViewModel
    {
        private readonly ICustomerService _customerService;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<CustomerDto> Customers { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private CustomerDto _selectedCustomer;
        public CustomerDto SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public ICommand LoadCustomersCommand { get; }
        public ICommand ShowAddCustomerDialogCommand { get; }
        public ICommand ShowEditCustomerDialogCommand { get; }
        public ICommand SettleBalanceCommand { get; }

        public CustomerManagementViewModel(ICustomerService customerService, IServiceProvider serviceProvider)
        {
            _customerService = customerService;
            _serviceProvider = serviceProvider;

            LoadCustomersCommand = new RelayCommand<object>(async (p) => await LoadCustomersAsync());
            ShowAddCustomerDialogCommand = new RelayCommand<object>((p) => ExecuteShowAddCustomerDialog());
            ShowEditCustomerDialogCommand = new RelayCommand<object>((p) => ExecuteShowEditCustomerDialog(), (p) => SelectedCustomer != null);
            SettleBalanceCommand = new RelayCommand<object>(async (p) => await ExecuteSettleBalanceAsync(), (p) => SelectedCustomer != null);

            LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            IsLoading = true;
            Customers.Clear();

            var result = await _customerService.GetAllCustomersAsync(default);
            if (result.IsSuccess)
            {
                foreach (var c in result.Value)
                    Customers.Add(c);
            }
            else
            {
                MessageBox.Show("فشل في تحميل العملاء:\n" + string.Join("\n", result.Errors),
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsLoading = false;
        }

        private void ExecuteShowAddCustomerDialog()
        {
            var viewModel = _serviceProvider.GetRequiredService<AddCustomerViewModel>();
            var view = new AddCustomerView(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            var result = view.ShowDialog();
            if (result == true && viewModel.NewCustomer != null)
                Customers.Add(viewModel.NewCustomer);
        }

        private async void ExecuteShowEditCustomerDialog()
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("يرجى اختيار عميل للتعديل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var viewModel = _serviceProvider.GetRequiredService<EditCustomerViewModel>();
            await viewModel.LoadCustomerDataAsync(SelectedCustomer.Id);

            var view = new EditCustomerView(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            var result = view.ShowDialog();
            if (result == true && viewModel.UpdatedCustomer != null)
            {
                var existing = Customers.FirstOrDefault(c => c.Id == viewModel.UpdatedCustomer.Id);
                if (existing != null)
                {
                    var index = Customers.IndexOf(existing);
                    Customers[index] = viewModel.UpdatedCustomer;
                }
            }
        }

        private async Task ExecuteSettleBalanceAsync()
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("اختر عميل لتسوية الرصيد.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"هل تريد تصفير رصيد العميل {SelectedCustomer.Name}؟", "تأكيد",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                var result = await _customerService.SettleCustomerBalanceAsync(SelectedCustomer.Id, default);
                if (result.IsSuccess)
                {
                    SelectedCustomer.CurrentBalance = 0;
                    MessageBox.Show("تمت تسوية الرصيد بنجاح ✅", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("فشل في تسوية الرصيد:\n" + string.Join("\n", result.Errors),
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
