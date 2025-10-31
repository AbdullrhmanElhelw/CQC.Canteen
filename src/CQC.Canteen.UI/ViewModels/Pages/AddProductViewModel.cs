using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.BusinessLogic.Services.Categories;
using CQC.Canteen.BusinessLogic.Services.Products;
using CQC.Canteen.UI.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages
{
    public class AddProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        private string _name;
        private decimal _salePrice;
        private decimal _purchasePrice;
        private int _stockQuantity;
        private int _selectedCategoryId;
        private string _errorMessage;
        private bool _isLoading;

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
        public ProductDto NewProduct { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public AddProductViewModel(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;

            SaveCommand = new RelayCommand<object>(async (p) => await ExecuteSaveAsync(), (p) => !IsLoading);
            CancelCommand = new RelayCommand<object>((p) => RequestClose?.Invoke(false), (p) => !IsLoading);

            LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            IsLoading = true;
            try
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
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSaveAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var createDto = new CreateProductDto
                {
                    Name = this.Name,
                    SalePrice = this.SalePrice,
                    PurchasePrice = this.PurchasePrice,
                    StockQuantity = this.StockQuantity,
                    CategoryId = this.SelectedCategoryId
                };

                var result = await _productService.AddNewProductAsync(createDto, default);

                if (result.IsSuccess)
                {
                    NewProduct = result.Value; // حفظ المنتج الجديد
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