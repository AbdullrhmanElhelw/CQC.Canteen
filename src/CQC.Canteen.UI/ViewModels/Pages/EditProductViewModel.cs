using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.BusinessLogic.Services.Categories;
using CQC.Canteen.BusinessLogic.Services.Products;
using CQC.Canteen.UI.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages
{
    public class EditProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        private int _productId;
        private string _name;
        private decimal _salePrice;
        private decimal _purchasePrice;
        private int _stockQuantity;
        private int _selectedCategoryId;
        private bool _isActive;
        private string _errorMessage;
        private bool _isLoading;

        public int ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal SalePrice
        {
            get => _salePrice;
            set => SetProperty(ref _salePrice, value);
        }

        public decimal PurchasePrice
        {
            get => _purchasePrice;
            set => SetProperty(ref _purchasePrice, value);
        }

        public int StockQuantity
        {
            get => _stockQuantity;
            set => SetProperty(ref _stockQuantity, value);
        }

        public int SelectedCategoryId
        {
            get => _selectedCategoryId;
            set => SetProperty(ref _selectedCategoryId, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<CategoryDto> Categories { get; } = new ObservableCollection<CategoryDto>();
        public ProductDto UpdatedProduct { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public EditProductViewModel(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;

            SaveCommand = new RelayCommand<object>(async (p) => await ExecuteSaveAsync(), (p) => !IsLoading);
            CancelCommand = new RelayCommand<object>((p) => RequestClose?.Invoke(false), (p) => !IsLoading);
        }

        public async Task LoadProductDataAsync(int productId)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // تحميل الفئات أولاً
                await LoadCategoriesAsync();

                // تحميل بيانات المنتج
                var productResult = await _productService.GetProductDetailsByIdAsync(productId, default);

                if (productResult.IsSuccess)
                {
                    var product = productResult.Value;
                    ProductId = product.Id;
                    Name = product.Name;
                    SalePrice = product.SalePrice;
                    PurchasePrice = product.PurchasePrice;
                    StockQuantity = product.StockQuantity;
                    SelectedCategoryId = product.CategoryId;
                    IsActive = product.IsActive;
                }
                else
                {
                    ErrorMessage = "فشل في تحميل بيانات المنتج: " + string.Join("\n", productResult.Errors);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"حدث خطأ أثناء تحميل البيانات: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var result = await _categoryService.GetAllCategoriesAsync(default);
            if (result.IsSuccess)
            {
                Categories.Clear();
                foreach (var category in result.Value)
                {
                    Categories.Add(category);
                }
            }
        }

        private async Task ExecuteSaveAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var updateDto = new ProductDetailsDto
                {
                    Id = this.ProductId,
                    Name = this.Name,
                    SalePrice = this.SalePrice,
                    PurchasePrice = this.PurchasePrice,
                    StockQuantity = this.StockQuantity,
                    CategoryId = this.SelectedCategoryId,
                    IsActive = this.IsActive
                };

                var result = await _productService.UpdateProductAsync(updateDto, default);

                if (result.IsSuccess)
                {
                    UpdatedProduct = result.Value;
                    RequestClose?.Invoke(true);
                }
                else
                {
                    ErrorMessage = string.Join("\n", result.Errors.Select(e => e.Message));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}