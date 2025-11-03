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
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
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
        public ObservableCollection<SaleItem> CartItems { get; } = new();
        public ObservableCollection<CategoryDto> Categories { get; } = new();
        public ICollectionView GroupedProductsView { get; }

        private CategoryDto? _selectedCategory;
        public CategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set { if (SetProperty(ref _selectedCategory, value)) ApplyFilter(); }
        }

        private string _productSearchText = "";
        public string ProductSearchText
        {
            get => _productSearchText;
            set { if (SetProperty(ref _productSearchText, value)) ApplyFilter(); }
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

        // *** تم التعديل هنا: إضافة الأوامر الجديدة ***
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }


        public CasherSalesViewModel(IProductService productService, IOrderService orderService, ICustomerService customerService, ICategoryService categoryService)
        {
            _productService = productService;
            _orderService = orderService;
            _customerService = customerService;
            _categoryService = categoryService;

            GroupedProductsView = CollectionViewSource.GetDefaultView(Products);
            GroupedProductsView.GroupDescriptions.Add(new PropertyGroupDescription("CategoryName"));
            GroupedProductsView.Filter = FilterProductsPredicate;

            AddToCartCommand = new RelayCommand<ProductDto>(AddToCart);
            RemoveFromCartCommand = new RelayCommand<SaleItem>(RemoveFromCart);
            PayCashCommand = new RelayCommand<object>(async _ => await ExecutePayCashAsync());
            PayDeferredCommand = new RelayCommand<object>(async _ => await ExecutePayDeferredAsync());

            // *** تم التعديل هنا: تهيئة الأوامر الجديدة ***
            IncreaseQuantityCommand = new RelayCommand<SaleItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<SaleItem>(DecreaseQuantity);
            ClearCartCommand = new RelayCommand<object>(ClearCart);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            var result = await _productService.GetAllProductsAsync(default);
            if (result.IsFailed)
            {
                MessageBox.Show(string.Join("\n", result.Errors), "خطأ في تحميل المنتجات", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Products.Clear();
            foreach (var p in result.Value.Where(p => p.IsActive))
                Products.Add(p);
        }

        private async Task LoadCategoriesAsync()
        {
            var result = await _categoryService.GetAllCategoriesAsync(default);
            Categories.Clear();
            Categories.Add(new CategoryDto { Name = "عرض الكل" });

            if (!result.IsFailed)
            {
                foreach (var category in result.Value)
                    Categories.Add(category);
            }
        }

        private void ApplyFilter()
        {
            GroupedProductsView.Refresh();
        }

        private bool FilterProductsPredicate(object obj)
        {
            if (obj is not ProductDto product)
                return false;

            bool matchesSearch = true;
            if (!string.IsNullOrWhiteSpace(ProductSearchText))
            {
                matchesSearch = (product.Name ?? "").Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase);
            }

            bool matchesCategory = true;
            if (SelectedCategory != null && SelectedCategory.Name != "عرض الكل")
            {
                matchesCategory = (product.CategoryName ?? "") == SelectedCategory.Name;
            }

            return matchesSearch && matchesCategory;
        }

        private void AddToCart(ProductDto product)
        {
            if (product == null) return;
            var existing = CartItems.FirstOrDefault(x => x.ProductId == product.Id);
            if (existing != null)
            {
                // تم التعديل لاستدعاء الميثود الجديدة
                IncreaseQuantity(existing);
            }
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

        public void UpdateTotal() => TotalAmount = CartItems.Sum(i => i.Total);


        // *** تم التعديل هنا: إضافة الميثودز الخاصة بالأوامر الجديدة ***

        /// <summary>
        /// زيادة كمية صنف في السلة
        /// </summary>
        private void IncreaseQuantity(SaleItem? item)
        {
            if (item == null) return;
            item.Quantity++;
            UpdateTotal();
            // ملحوظة: لو عندك كمية في المخزن، ده مكان التحقق منها
        }

        /// <summary>
        /// تقليل كمية صنف في السلة أو حذفه لو وصل لـ 1
        /// </summary>
        private void DecreaseQuantity(SaleItem? item)
        {
            if (item == null) return;
            if (item.Quantity > 1)
            {
                item.Quantity--;
                UpdateTotal();
            }
            else if (item.Quantity == 1)
            {
                // الحذف التلقائي لو الكمية هتوصل صفر
                RemoveFromCart(item);
            }
        }

        /// <summary>
        /// إفراغ سلة المشتريات بالكامل
        /// </summary>
        private void ClearCart(object? _ = null)
        {
            if (!CartItems.Any()) return;

            if (MessageBox.Show("هل أنت متأكد أنك تريد إفراغ السلة؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                CartItems.Clear();
                UpdateTotal();
            }
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
                CreatedByUserId = 1, //TODO: استبدل بالـ ID الخاص بالمستخدم المسجل
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
                MessageBox.Show("✅ تم تسجيل البيع النقدي بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                CartItems.Clear();
                UpdateTotal();
            }
            //TODO: معالجة الأخطاء هنا
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
                CreatedByUserId = 1, //TODO: استبدل بالـ ID الخاص بالمستخدم المسجل
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
                MessageBox.Show($"✅ تم تسجيل البيع الآجل باسم {selectedCustomer.Name}", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                CartItems.Clear();
                UpdateTotal();
            }
            //TODO: معالجة الأخطاء هنا
        }
    }

    public class SaleItem : BaseViewModel
    {
        public int ProductId { get; set; }
        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private int _quantity;
        public int Quantity { get => _quantity; set { if (SetProperty(ref _quantity, value)) OnPropertyChanged(nameof(Total)); } }
        private decimal _price;
        public decimal Price { get => _price; set { if (SetProperty(ref _price, value)) OnPropertyChanged(nameof(Total)); } }
        public decimal Total => Quantity * Price;
    }
}