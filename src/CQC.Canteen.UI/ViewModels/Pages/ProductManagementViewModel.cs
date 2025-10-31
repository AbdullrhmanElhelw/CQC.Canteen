using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.BusinessLogic.Services.Products;
using CQC.Canteen.UI.Commands;
using CQC.Canteen.UI.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages
{
    public class ProductManagementViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<ProductDto> Products { get; } = new ObservableCollection<ProductDto>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private ProductDto _selectedProduct;
        public ProductDto SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        public ICommand LoadProductsCommand { get; }
        public ICommand ShowAddProductDialogCommand { get; }
        public ICommand ShowEditProductDialogCommand { get; }

        public ProductManagementViewModel(IProductService productService, IServiceProvider serviceProvider)
        {
            _productService = productService;
            _serviceProvider = serviceProvider;

            LoadProductsCommand = new RelayCommand<object>(async (p) => await LoadProductsAsync());
            ShowAddProductDialogCommand = new RelayCommand<object>((p) => ExecuteShowAddProductDialog());
            ShowEditProductDialogCommand = new RelayCommand<object>((p) => ExecuteShowEditProductDialog(), (p) => SelectedProduct != null);

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
                MessageBox.Show("فشل في تحميل الأصناف: " + string.Join("\n", result.Errors),
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsLoading = false;
        }

        private void ExecuteShowAddProductDialog()
        {
            var viewModel = _serviceProvider.GetRequiredService<AddProductViewModel>();
            var view = new AddProductView(viewModel);
            view.Owner = Application.Current.MainWindow;

            var dialogResult = view.ShowDialog();

            if (dialogResult == true && viewModel.NewProduct != null)
            {
                Products.Add(viewModel.NewProduct);
            }
        }

        private async void ExecuteShowEditProductDialog()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("يرجى اختيار صنف للتعديل", "تنبيه",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var viewModel = _serviceProvider.GetRequiredService<EditProductViewModel>();
            var view = new EditProductView(viewModel);
            view.Owner = Application.Current.MainWindow;

            // تحميل بيانات المنتج قبل عرض النافذة
            await viewModel.LoadProductDataAsync(SelectedProduct.Id);

            var dialogResult = view.ShowDialog();

            if (dialogResult == true && viewModel.UpdatedProduct != null)
            {
                // تحديث العنصر في القائمة
                var existingProduct = Products.FirstOrDefault(p => p.Id == viewModel.UpdatedProduct.Id);
                if (existingProduct != null)
                {
                    var index = Products.IndexOf(existingProduct);
                    Products[index] = viewModel.UpdatedProduct;
                }
            }
        }
    }
}