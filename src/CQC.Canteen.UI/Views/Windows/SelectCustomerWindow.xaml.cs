using CQC.Canteen.BusinessLogic.DTOs.Customers;
using CQC.Canteen.BusinessLogic.Services.Customers;
using System.Collections.ObjectModel;
using System.Windows;

namespace CQC.Canteen.UI.Views.Windows
{
    public partial class SelectCustomerWindow : Window
    {
        private readonly ICustomerService _customerService;
        private ObservableCollection<CustomerDto> _customers;
        public CustomerDto? SelectedCustomer { get; private set; }

        public SelectCustomerWindow(ICustomerService customerService)
        {
            InitializeComponent();
            _customerService = customerService;
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            var result = await _customerService.GetAllCustomersAsync(default);
            if (result.IsSuccess)
            {
                _customers = new ObservableCollection<CustomerDto>(result.Value.Where(c => c.IsActive));
                CustomerList.ItemsSource = _customers;
            }
        }

        private void OnSearchTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_customers == null) return;
            var query = SearchBox.Text.ToLower();
            CustomerList.ItemsSource = _customers.Where(c => c.Name.ToLower().Contains(query));
        }

        private void CustomerList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmSelection();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (CustomerList.SelectedItem is CustomerDto selected)
            {
                SelectedCustomer = selected;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("من فضلك اختر عميلًا أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
