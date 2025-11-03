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
    private readonly IPrintingService _printingService; // *** (1. تمت الإضافة) ***

    public ObservableCollection<ProductDto> Products { get; } = new();
    public ObservableCollection<SaleItem> CartItems { get; } = new();
    public ObservableCollection<CategoryDto> Categories { get; } = new();
    public ICollectionView GroupedProductsView { get; }

    // ... (الخصائص الأخرى كما هي: SelectedCategory, ProductSearchText, TotalAmount) ...
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

    public ICommand AddToCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand PayCashCommand { get; }
    public ICommand PayDeferredCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand ClearCartCommand { get; }


    // *** (2. تعديل الـ Constructor) ***
    public CasherSalesViewModel(
        IProductService productService,
        IOrderService orderService,
        ICustomerService customerService,
        ICategoryService categoryService,
        IPrintingService printingService) // (تمت إضافة سيرفس الطباعة)
    {
        _productService = productService;
        _orderService = orderService;
        _customerService = customerService;
        _categoryService = categoryService;
        _printingService = printingService; // (تمت الإضافة)

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

        _ = InitializeAsync();
    }

    // ... (الدوال المساعدة كما هي: InitializeAsync, LoadProductsAsync, LoadCategoriesAsync) ...
    #region Loading and Filtering
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
    #endregion

    // ... (دوال السلة كما هي: CanAddToCart, AddToCart, RemoveFromCart, UpdateTotal, IncreaseQuantity, DecreaseQuantity, ClearCart) ...
    #region Cart Logic
    private bool CanAddToCart(ProductDto? product)
    {
        if (product == null)
            return false;
        return product.StockQuantity > 0;
    }

    private void AddToCart(ProductDto product)
    {
        if (product == null) return;
        if (product.StockQuantity <= 0)
        {
            MessageBox.Show("عفواً، هذا المنتج نفذت كميته.", "نفذت الكمية", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var existing = CartItems.FirstOrDefault(x => x.ProductId == product.Id);
        if (existing != null)
        {
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
        {
            item.Quantity--;
            UpdateTotal();
        }
        else if (item.Quantity == 1)
        {
            RemoveFromCart(item);
        }
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

    // *** (3. تعديل دالة الدفع النقدي) ***
    private async Task ExecutePayCashAsync()
    {
        if (!CartItems.Any())
        {
            MessageBox.Show("السلة فارغة!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 1. فتح شاشة الدفع النقدي الجديدة
        var cashDialog = new PayCashWindow(this.TotalAmount);

        // 2. لو المستخدم ضغط "إلغاء"، اخرج
        if (cashDialog.ShowDialog() != true)
        {
            return;
        }

        // 3. قراءة البيانات من الشاشة بعد التأكيد
        var shouldPrint = cashDialog.ShouldPrintReceipt;
        var amountReceived = cashDialog.AmountReceived;
        var change = cashDialog.Change;

        // 4. إنشاء الـ DTO الخاص بالحفظ
        var dto = new CreateOrderDto
        {
            CreatedByUserId = 1, //TODO: استبدل بالـ ID الخاص بالمستخدم المسجل
            PaymentMethod = PaymentMethod.Cash,
            Items = CartItems.Select(c => new OrderItemDto
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Price
            }).ToList(),
            // لو الـ DTO بيدعمها، أضف المبلغ المدفوع (هو نفسه الإجمالي في حالة الكاش)
            AmountPaid = this.TotalAmount
        };

        // *** (4. تحويل الأصناف للطباعة) ***
        // (نعمل نسخة قبل تفريغ السلة)
        var itemsToPrint = CartItems.Select(item => new SaleItemDto
        {
            ProductId = item.ProductId,
            Name = item.Name,
            Quantity = item.Quantity,
            Price = item.Price
        }).ToList();

        // 5. حفظ الأوردر في الداتا بيز
        var result = await _orderService.CreateCashOrderAsync(dto, default);

        if (result.IsSuccess)
        {
            // 6. لو الحفظ نجح، نفذ الطباعة
            if (shouldPrint)
            {
                try
                {
                    // استخدام البيانات من شاشة الدفع
                    await _printingService.PrintReceiptAsync(itemsToPrint, this.TotalAmount, amountReceived, change);
                    MessageBox.Show("✅ تم تسجيل البيع ... وجاري طباعة الإيصال", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"تم البيع، لكن فشلت الطباعة: {ex.Message}", "خطأ طباعة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("✅ تم تسجيل البيع النقدي بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // 7. تفريغ السلة
            CartItems.Clear();
            UpdateTotal();
        }
        else
        {
            //TODO: معالجة أخطاء الحفظ
            MessageBox.Show(string.Join("\n", result.Errors), "خطأ في حفظ الأوردر", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // *** (5. تعديل دالة الدفع الآجل للطباعة أيضاً) ***
    private async Task ExecutePayDeferredAsync()
    {
        if (!CartItems.Any())
        {
            MessageBox.Show("السلة فارغة!", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 1. فتح شاشة اختيار العميل
        var dialog = new SelectCustomerWindow(_customerService, this.TotalAmount);

        if (dialog.ShowDialog() != true) return;

        var selectedCustomer = dialog.SelectedCustomer;
        var amountPaid = dialog.AmountPaid; // المبلغ المدفوع جزئياً

        if (selectedCustomer == null) return;

        // (إضافة) سؤال عن الطباعة للدفع الآجل
        var printResult = MessageBox.Show("هل تريد طباعة إيصال لهذه العملية؟", "تأكيد الطباعة", MessageBoxButton.YesNo, MessageBoxImage.Question);
        bool shouldPrint = (printResult == MessageBoxResult.Yes);

        // 2. إنشاء الـ DTO
        var dto = new CreateOrderDto
        {
            CreatedByUserId = 1, //TODO:
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

        // (إضافة) تحويل الأصناف للطباعة
        var itemsToPrint = CartItems.Select(item => new SaleItemDto
        {
            ProductId = item.ProductId,
            Name = item.Name,
            Quantity = item.Quantity,
            Price = item.Price
        }).ToList();

        // 3. حفظ الأوردر
        var result = await _orderService.CreateDeferredOrderAsync(dto, default);

        if (result.IsSuccess)
        {
            decimal remaining = this.TotalAmount - amountPaid;

            // 4. تنفيذ الطباعة (لو اختار نعم)
            if (shouldPrint)
            {
                try
                {
                    // هنا نرسل المبلغ المدفوع (الجزئي) والمتبقي
                    await _printingService.PrintReceiptAsync(itemsToPrint, this.TotalAmount, amountPaid, remaining);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"تم البيع، لكن فشلت الطباعة: {ex.Message}", "خطأ طباعة", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // 5. إظهار رسالة النجاح
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
            //TODO: معالجة أخطاء الحفظ
            MessageBox.Show(string.Join("\n", result.Errors), "خطأ في حفظ الأوردر", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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