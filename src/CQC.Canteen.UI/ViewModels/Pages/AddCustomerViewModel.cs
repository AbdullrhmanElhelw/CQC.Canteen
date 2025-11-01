using CQC.Canteen.BusinessLogic.DTOs.Customers;
using CQC.Canteen.BusinessLogic.Services.Customers;
using CQC.Canteen.Domain.Enums;
using CQC.Canteen.UI.Commands;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages
{
    public class AddCustomerViewModel : BaseViewModel
    {
        private readonly ICustomerService _customerService;

        // 🧩 الخصائص القابلة للربط
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool _isMilitary;
        public bool IsMilitary
        {
            get => _isMilitary;
            set => SetProperty(ref _isMilitary, value);
        }

        private MilitaryRank? _rank;
        public MilitaryRank? Rank
        {
            get => _rank;
            set => SetProperty(ref _rank, value);
        }

        // 🔁 النتيجة بعد الحفظ
        public CustomerDto? NewCustomer { get; private set; }

        // 🧠 الأوامر
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // 🧱 الـ Constructor
        public AddCustomerViewModel(ICustomerService customerService)
        {
            _customerService = customerService;

            SaveCommand = new RelayCommand<object>(async (p) => await ExecuteSaveAsync(p));
            CancelCommand = new RelayCommand<object>((p) =>
            {
                if (p is Window window)
                    window.Close();
            });
        }

        // 💾 تنفيذ الحفظ
        private async Task ExecuteSaveAsync(object parameter)
        {
            // التحقق من صحة البيانات قبل الإرسال
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("يرجى إدخال اسم العميل.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsMilitary && Rank == null)
            {
                MessageBox.Show("يجب اختيار الرتبة العسكرية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new CreateCustomerDto
            {
                Name = Name,
                IsMilitary = IsMilitary,
                Rank = IsMilitary ? Rank : null
            };

            var result = await _customerService.AddCustomerAsync(dto, default);

            if (result.IsSuccess)
            {
                NewCustomer = result.Value;
                MessageBox.Show("✅ تم إضافة العميل بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
            {
                MessageBox.Show(string.Join("\n", result.Errors),
                                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
