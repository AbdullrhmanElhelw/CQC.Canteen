using CQC.Canteen.BusinessLogic.DTOs.Customers;
using CQC.Canteen.BusinessLogic.Services.Customers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CQC.Canteen.UI.Views.Windows
{
    public partial class SelectCustomerWindow : Window
    {
        private readonly ICustomerService _customerService;
        private readonly ICollectionView _customerView;
        private readonly ObservableCollection<CustomerDto> _allCustomers = new();

        private readonly decimal _totalBillAmount;

        public decimal AmountPaid { get; private set; }
        public CustomerDto? SelectedCustomer { get; private set; }

        // *** تعديل: استقبال إجمالي الفاتورة ***
        public SelectCustomerWindow(ICustomerService customerService, decimal totalBillAmount)
        {
            InitializeComponent();
            _customerService = customerService;
            _totalBillAmount = totalBillAmount;

            _customerView = CollectionViewSource.GetDefaultView(_allCustomers);
            _customerView.Filter = FilterCustomers;
            CustomerList.ItemsSource = _customerView;

            // *** إضافة: تحديث الواجهة بقيم الدفع ***
            TotalBillText.Text = _totalBillAmount.ToString("N2") + " ج.م";
            AmountPaidBox.Text = "0";
            UpdateRemainingAmount();

            _ = LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            var result = await _customerService.GetAllCustomersAsync(default);
            if (result.IsSuccess)
            {
                _allCustomers.Clear();
                foreach (var customer in result.Value.Where(c => c.IsActive))
                {
                    _allCustomers.Add(customer);
                }
            }
            //TODO: Handle failure
        }

        private bool FilterCustomers(object obj)
        {
            if (obj is not CustomerDto customer) return false;
            string searchText = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText)) return true;
            return (customer.Name ?? "").Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _customerView.Refresh();
        }

        // *** إضافة: دالة حساب المتبقي ***
        private void UpdateRemainingAmount()
        {
            if (!decimal.TryParse(AmountPaidBox.Text, out decimal paid))
            {
                paid = 0;
            }

            decimal remaining = _totalBillAmount - paid;
            RemainingAmountText.Text = remaining.ToString("N2") + " ج.م";

            if (remaining > 0)
            {
                RemainingAmountText.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                RemainingAmountText.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        // *** إضافة: تفعيل الحساب مع كل تغيير ***
        private void AmountPaidBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RemainingAmountText != null)
            {
                UpdateRemainingAmount();
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void CustomerList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CustomerList.SelectedItem != null)
            {
                ConfirmSelection();
            }
        }

        // *** تعديل: إضافة التحقق من الدفع قبل الإغلاق ***
        private void ConfirmSelection()
        {
            SelectedCustomer = CustomerList.SelectedItem as CustomerDto;
            if (SelectedCustomer == null)
            {
                MessageBox.Show("الرجاء اختيار العميل أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(AmountPaidBox.Text, out decimal paid) || paid < 0)
            {
                MessageBox.Show("الرجاء إدخال مبلغ مدفوع صحيح (رقم موجب).", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (paid > _totalBillAmount)
            {
                MessageBox.Show("المبلغ المدفوع لا يمكن أن يكون أكبر من إجمالي الفاتورة.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.AmountPaid = paid;

            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}