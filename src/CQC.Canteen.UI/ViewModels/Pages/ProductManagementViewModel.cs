using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.BusinessLogic.Services.Products;
using CQC.Canteen.UI.Commands;
using CQC.Canteen.UI.Views.Pages; // <-- (1) إضافة مهمة
using Microsoft.Extensions.DependencyInjection; // <-- (2) إضافة مهمة
using System.Collections.ObjectModel;
using System.Windows; // <-- (4) إضافة مهمة
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages
{
    public class ProductManagementViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly IServiceProvider _serviceProvider; // <-- (5) إضافة

        public ObservableCollection<ProductDto> Products { get; } = new ObservableCollection<ProductDto>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadProductsCommand { get; }
        public ICommand ShowAddProductDialogCommand { get; } // <-- (6) إضافة

        public ProductManagementViewModel(IProductService productService, IServiceProvider serviceProvider)
        {
            _productService = productService;
            _serviceProvider = serviceProvider; // <-- (7) إضافة

            LoadProductsCommand = new RelayCommand<object>(async (p) => await LoadProductsAsync());
            ShowAddProductDialogCommand = new RelayCommand<object>((p) => ExecuteShowAddProductDialog()); // <-- (8) إضافة

            LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            IsLoading = true;
            Products.Clear();

            var result = await _productService.GetAllProductsAsync(default);

            if (result.IsSuccess)
            {
                foreach (var product in result.Value)
                {
                    Products.Add(product);
                }
            }
            else
            {
                // (ممكن نعرض رسالة خطأ)
            }

            IsLoading = false;
        }

        // (9) الميثود الجديدة اللي بتفتح الـ Popup
        private void ExecuteShowAddProductDialog()
        {
            // 1. اطلب VM جديد من الـ DI (عشان يكون نضيف)
            var viewModel = _serviceProvider.GetRequiredService<AddProductViewModel>();

            // 2. اعمل شاشة جديدة واديهالها
            var view = new AddProductView(viewModel);

            // 3. خليها تفتح فوق الشاشة الرئيسية
            view.Owner = Application.Current.MainWindow;

            // 4. اعرضها وانتظر النتيجة
            var dialogResult = view.ShowDialog();

            // 5. لو النتيجة "True" (يعني داس حفظ)
            if (dialogResult == true && viewModel.NewProduct != null)
            {
                // ضيف الصنف الجديد للجدول اللي في الشاشة
                Products.Add(viewModel.NewProduct);
            }
        }
    }
}

