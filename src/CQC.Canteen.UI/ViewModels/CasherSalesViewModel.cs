using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.BusinessLogic.DTOs.Orders;
using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.BusinessLogic.Services.Categories;
using CQC.Canteen.BusinessLogic.Services.Customers;
using CQC.Canteen.BusinessLogic.Services.Orders;
using CQC.Canteen.BusinessLogic.Services.Printing;
using CQC.Canteen.BusinessLogic.Services.Products;
using CQC.Canteen.Domain.Enums;
using CQC.Canteen.UI.Commands;
using CQC.Canteen.UI.Views.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels;

public class CasherSalesViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly ICategoryService _categoryService;
    private readonly IPrintingService _printingService;

    public ObservableCollection<ProductDto> Products { get; } = new();
    public ObservableCollection<SaleItem> CartItems { get; } = new();
    public ObservableCollection<CategoryDto> Categories { get; } = new();
    public ICollectionView GroupedProductsView { get; }

    #region Properties
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
    #endregion

    #region Static Event for Product Refresh
    public static event EventHandler? ProductsUpdated;
    #endregion

    public ICommand AddToCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand PayCashCommand { get; }
    public ICommand PayDeferredCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand ClearCartCommand { get; }

    public CasherSalesViewModel(
        IProductService productService,
        IOrderService orderService,
        ICustomerService customerService,
        ICategoryService categoryService,
        IPrintingService printingService)
    {
        _productService = productService;
        _orderService = orderService;
        _customerService = customerService;
        _categoryService = categoryService;
        _printingService = printingService;

        GroupedProductsView = CollectionViewSource.GetDefaultView(Products);
        GroupedProductsView.GroupDescriptions.Add(new PropertyGroupDescription("CategoryName"));
        GroupedProductsView.Filter = FilterProductsPredicate;

        AddToCartCommand = new RelayCommand<ProductDto>(AddToCart, CanAddToCart);
        RemoveFromCartCommand = new RelayCommand<SaleItem>(RemoveFromCart);
        PayCashCommand = new RelayCommand<object>(async _ => await ExecutePayCashAsync());
        PayDeferredCommand = new RelayCommand<object>(async _ => await ExecutePayDeferredAsync());
        IncreaseQuantityCommand = new RelayCommand<SaleItem>(IncreaseQuantity);
        DecreaseQuantityCommand = new RelayCommand<SaleItem>(DecreaseQuantity);
        ClearCartCommand = new RelayCommand<object>(ClearCart);

        // 🔔 اشترك في الحدث العام لتحديث المنتجات عند أي بيع
        ProductsUpdated += async (_, __) => await LoadProductsAsync();

        _ = InitializeAsync();
    }

    #region Initialization & Filtering
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

        ApplyFilter();
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

    private void ApplyFilter() => GroupedProductsView.Refresh();

    private bool FilterProductsPredicate(object obj)
    {
        if (obj is not ProductDto product)
            return false;

        bool matchesSearch = string.IsNullOrWhiteSpace(ProductSearchText)
            || (product.Name ?? "").Contains(ProductSearchText, StringComparison.OrdinalIgnoreCase);

        bool matchesCategory = SelectedCategory == null
            || SelectedCategory.Name == "عرض الكل"
            || (product.CategoryName ?? "") == SelectedCategory.Name;

        return matchesSearch && matchesCategory;
    }
    #endregion

    #region Cart Logic
    private bool CanAddToCart(ProductDto? product)
        => product != null && product.StockQuantity > 0;

    private void AddToCart(ProductDto product)
    {
        if (product.StockQuantity <= 0)
        {
            MessageBox.Show("عفواً، هذا المنتج نفذت كميته.", "نفذت الكمية", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var existing = CartItems.FirstOrDefault(x => x.ProductId == product.Id);
        if (existing != null)
            IncreaseQuantity(existing);
        else
            CartItems.Add(new SaleItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.SalePrice,
                Quantity = 1
            });

        UpdateTotal();
    }

    private void RemoveFromCart(SaleItem item)
    {
        CartItems.Remove(item);
        UpdateTotal();
    }

    public void UpdateTotal() => TotalAmount = CartItems.Sum(i => i.Total);

    private void IncreaseQuantity(SaleItem? item)
    {
        if (item == null) return;
        var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
        if (product != null && item.Quantity >= product.StockQuantity)
        {
            MessageBox.Show($"لا يمكن إضافة المزيد، الكمية المتاحة هي {product.StockQuantity} فقط.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        item.Quantity++;
        UpdateTotal();
    }

    private void DecreaseQuantity(SaleItem? item)
    {
        if (item == null) return;
        if (item.Quantity > 1)
            item.Quantity--;
        else
            CartItems.Remove(item);

        UpdateTotal();
    }

    private void ClearCart(object? _ = null)
    {
        if (!CartItems.Any()) return;
        if (MessageBox.Show("هل أنت متأكد أنك تريد إفراغ السلة؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            CartItems.Clear();
            UpdateTotal();
        }
    }
    #endregion

    #region Payment Logic
    private async Task ExecutePayCashAsync()
    {
        if (!CartItems.Any())
        {
            MessageBox.Show("السلة فارغة!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var cashDialog = new PayCashWindow(this.TotalAmount);
        if (cashDialog.ShowDialog() != true) return;

        var shouldPrint = cashDialog.ShouldPrintReceipt;
        var amountReceived = cashDialog.AmountReceived;
        var change = cashDialog.Change;

        var dto = new CreateOrderDto
        {
            CreatedByUserId = 1, // TODO
            PaymentMethod = PaymentMethod.Cash,
            Items = CartItems.Select(c => new OrderItemDto
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Price
            }).ToList(),
            AmountPaid = this.TotalAmount
        };

        var itemsToPrint = CartItems.Select(item => new SaleItemDto
        {
            ProductId = item.ProductId,
            Name = item.Name,
            Quantity = item.Quantity,
            Price = item.Price
        }).ToList();

        var result = await _orderService.CreateCashOrderAsync(dto, default);

        if (result.IsSuccess)
        {
            if (shouldPrint)
            {
                try
                {
                    await _printingService.PrintReceiptAsync(itemsToPrint, this.TotalAmount, amountReceived, change);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"تم البيع، لكن فشلت الطباعة: {ex.Message}", "خطأ طباعة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            MessageBox.Show("✅ تم تسجيل البيع النقدي بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

            // 🔄 تحديث المنتجات بعد البيع
            await LoadProductsAsync();
            ProductsUpdated?.Invoke(this, EventArgs.Empty);

            CartItems.Clear();
            UpdateTotal();
        }
        else
        {
            MessageBox.Show(string.Join("\n", result.Errors), "خطأ في حفظ الأوردر", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExecutePayDeferredAsync()
    {
        if (!CartItems.Any())
        {
            MessageBox.Show("السلة فارغة!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SelectCustomerWindow(_customerService, this.TotalAmount);
        if (dialog.ShowDialog() != true) return;

        var selectedCustomer = dialog.SelectedCustomer;
        var amountPaid = dialog.AmountPaid;
        if (selectedCustomer == null) return;

        var printResult = MessageBox.Show("هل تريد طباعة إيصال لهذه العملية؟", "تأكيد الطباعة", MessageBoxButton.YesNo, MessageBoxImage.Question);
        bool shouldPrint = (printResult == MessageBoxResult.Yes);

        var dto = new CreateOrderDto
        {
            CreatedByUserId = 1, // TODO
            PaymentMethod = PaymentMethod.Deferred,
            CustomerId = selectedCustomer.Id,
            Items = CartItems.Select(c => new OrderItemDto
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Price
            }).ToList(),
            AmountPaid = amountPaid
        };

        var itemsToPrint = CartItems.Select(item => new SaleItemDto
        {
            ProductId = item.ProductId,
            Name = item.Name,
            Quantity = item.Quantity,
            Price = item.Price
        }).ToList();

        var result = await _orderService.CreateDeferredOrderAsync(dto, default);

        if (result.IsSuccess)
        {
            decimal remaining = this.TotalAmount - amountPaid;

            if (shouldPrint)
            {
                try
                {
                    await _printingService.PrintReceiptAsync(itemsToPrint, this.TotalAmount, amountPaid, remaining);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"تم البيع، لكن فشلت الطباعة: {ex.Message}", "خطأ طباعة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            await LoadProductsAsync();
            ProductsUpdated?.Invoke(this, EventArgs.Empty);

            MessageBox.Show($"✅ تم تسجيل البيع الآجل باسم {selectedCustomer.Name}" +
                            $"\nالإجمالي: {this.TotalAmount:N2} ج.م" +
                            $"\nالمدفوع: {amountPaid:N2} ج.م" +
                            $"\nالمتبقي (دين): {remaining:N2} ج.م",
                            "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

            CartItems.Clear();
            UpdateTotal();
        }
        else
        {
            MessageBox.Show(string.Join("\n", result.Errors), "خطأ في حفظ الأوردر", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    #endregion
}

public class SaleItem : BaseViewModel
{
    public int ProductId { get; set; }

    private string _name = "";
    public string Name { get => _name; set => SetProperty(ref _name, value); }

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
