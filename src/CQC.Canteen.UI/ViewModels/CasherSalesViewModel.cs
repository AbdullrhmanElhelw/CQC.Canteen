using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.BusinessLogic.DTOs.Orders;
using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.BusinessLogic.Services.Categories;
using CQC.Canteen.BusinessLogic.Services.Customers;
using CQC.Canteen.BusinessLogic.Services.Orders;
using CQC.Canteen.BusinessLogic.Services.Products;
using CQC.Canteen.Domain.Enums;
using CQC.Canteen.UI.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels
{
    public class CasherSalesViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ICategoryService _categoryService;

        public ObservableCollection<ProductDto> Products { get; } = new();
        public ObservableCollection<ProductDto> FilteredProducts { get; } = new();
        public ObservableCollection<SaleItem> CartItems { get; } = new();
        public ObservableCollection<CategoryDto> Categories { get; } = new();

        private string _productSearchText = "";
        public string ProductSearchText
        {
            get => _productSearchText;
            set { if (SetProperty(ref _productSearchText, value)) ApplyFilter(); }
        }

        private decimal? _minPrice;
        public decimal? MinPrice
        {
            get => _minPrice;
            set { if (SetProperty(ref _minPrice, value)) ApplyFilter(); }
        }

        private decimal? _maxPrice;
        public decimal? MaxPrice
        {
            get => _maxPrice;
            set { if (SetProperty(ref _maxPrice, value)) ApplyFilter(); }
        }

        private CategoryDto? _selectedCategory;
        public CategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set { if (SetProperty(ref _selectedCategory, value)) ApplyFilter(); }
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set { if (SetProperty(ref _totalAmount, value)) TotalAmountChanged?.Invoke(this, value); }
        }

        public event EventHandler<decimal>? TotalAmountChanged;

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand PayCashCommand { get; }
        public ICommand PayDeferredCommand { get; }

        public CasherSalesViewModel(
            IProductService productService,
            IOrderService orderService,
            ICustomerService customerService,
            ICategoryService categoryService)
        {
            _productService = productService;
            _orderService = orderService;
            _customerService = customerService;
            _categoryService = categoryService;

            AddToCartCommand = new RelayCommand<ProductDto>(AddToCart);
            RemoveFromCartCommand = new RelayCommand<SaleItem>(RemoveFromCart);
            PayCashCommand = new RelayCommand<object>(async _ => await ExecutePayCashAsync());
            PayDeferredCommand = new RelayCommand<object>(async _ => await ExecutePayDeferredAsync());

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();

            // ✅ تحديد أول فئة تلقائيًا
            if (Categories.Any())
                SelectedCategory = Categories.First();
        }

        private async Task LoadProductsAsync()
        {
            var result = await _productService.GetAllProductsAsync(default);
            if (result.IsFailed)
            {
                MessageBox.Show(string.Join("\n", result.Errors),
                    "❌ خطأ في تحميل المنتجات", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Products.Clear();
            foreach (var p in result.Value.Where(p => p.IsActive))
                Products.Add(p);

            ApplyFilter();
        }

        private async Task LoadCategoriesAsync()
        {
            var result = await _categoryService.GetAllCategoriesAsync(default);

            Categories.Clear();
            Categories.Add(new CategoryDto { Name = "عرض الكل" }); // ✅ الفئة الافتراضية

            if (result.IsFailed)
            {
                MessageBox.Show(string.Join("\n", result.Errors),
                    "⚠️ خطأ في تحميل الفئات", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var category in result.Value)
                Categories.Add(category);
        }

        private void ApplyFilter()
        {
            var filtered = Products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ProductSearchText))
                filtered = filtered.Where(p => p.Name.Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase));

            if (SelectedCategory != null && SelectedCategory.Name != "عرض الكل")
                filtered = filtered.Where(p => p.CategoryName == SelectedCategory.Name);

            if (MinPrice.HasValue)
                filtered = filtered.Where(p => p.SalePrice >= MinPrice.Value);

            if (MaxPrice.HasValue)
                filtered = filtered.Where(p => p.SalePrice <= MaxPrice.Value);

            FilteredProducts.Clear();
            foreach (var item in filtered)
                FilteredProducts.Add(item);
        }

        public void UpdateTotal()
        {
            TotalAmount = CartItems.Sum(i => i.Total);
        }

        private void AddToCart(ProductDto product)
        {
            if (product == null) return;

            var existing = CartItems.FirstOrDefault(x => x.ProductId == product.Id);
            if (existing != null)
                existing.Quantity++;
            else
            {
                CartItems.Add(new SaleItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.SalePrice,
                    Quantity = 1
                });
            }

            UpdateTotal();
        }

        private void RemoveFromCart(SaleItem item)
        {
            if (item == null) return;
            CartItems.Remove(item);
            UpdateTotal();
        }

        private async Task ExecutePayCashAsync()
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("السلة فارغة!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new CreateOrderDto
            {
                CreatedByUserId = 1,
                PaymentMethod = PaymentMethod.Cash,
                Items = CartItems.Select(c => new OrderItemDto
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Price
                }).ToList()
            };

            var result = await _orderService.CreateCashOrderAsync(dto, default);
            if (result.IsSuccess)
            {
                MessageBox.Show("✅ تم تسجيل عملية البيع النقدي بنجاح",
                    "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                CartItems.Clear();
                UpdateTotal();
            }
            else
            {
                MessageBox.Show(string.Join("\n", result.Errors),
                    "خطأ أثناء حفظ الطلب", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecutePayDeferredAsync()
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("السلة فارغة!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Views.Windows.SelectCustomerWindow(_customerService);
            if (dialog.ShowDialog() != true) return;

            var selectedCustomer = dialog.SelectedCustomer;
            if (selectedCustomer == null) return;

            var dto = new CreateOrderDto
            {
                CreatedByUserId = 1,
                PaymentMethod = PaymentMethod.Deferred,
                CustomerId = selectedCustomer.Id,
                Items = CartItems.Select(c => new OrderItemDto
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Price
                }).ToList()
            };

            var result = await _orderService.CreateDeferredOrderAsync(dto, default);
            if (result.IsSuccess)
            {
                MessageBox.Show($"✅ تم تسجيل البيع الآجل باسم {selectedCustomer.Name}",
                    "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                CartItems.Clear();
                UpdateTotal();
            }
            else
            {
                MessageBox.Show(string.Join("\n", result.Errors),
                    "خطأ أثناء تسجيل الطلب", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class SaleItem : BaseViewModel
    {
        public int ProductId { get; set; }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { if (SetProperty(ref _quantity, value)) OnPropertyChanged(nameof(Total)); }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set { if (SetProperty(ref _price, value)) OnPropertyChanged(nameof(Total)); }
        }

        public decimal Total => Quantity * Price;
    }
}
