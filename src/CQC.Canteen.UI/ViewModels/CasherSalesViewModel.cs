using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.BusinessLogic.Services.Products;
using CQC.Canteen.UI.Commands;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels
{
    public class CasherSalesViewModel : BaseViewModel
    {
        private readonly IProductService _productService;

        public ObservableCollection<ProductDto> Products { get; } = new();
        public ObservableCollection<SaleItem> CartItems { get; } = new();

        public decimal TotalAmount => CartItems.Sum(i => i.Total);

        public ICommand AddToCartCommand { get; }

        public CasherSalesViewModel(IProductService productService)
        {
            _productService = productService;
            AddToCartCommand = new RelayCommand<ProductDto>(AddToCart);

            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            var result = await _productService.GetAllProductsAsync(default);
            if (result.IsFailed)
            {
                MessageBox.Show(string.Join("\n", result.Errors), "خطأ في تحميل الأصناف", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Products.Clear();
            foreach (var p in result.Value.Where(p => p.IsActive))
                Products.Add(p);
        }

        private void AddToCart(ProductDto product)
        {
            var existing = CartItems.FirstOrDefault(x => x.Name == product.Name);
            if (existing != null)
                existing.Quantity++;
            else
                CartItems.Add(new SaleItem
                {
                    Name = product.Name,
                    Price = product.SalePrice,
                    Quantity = 1
                });

            OnPropertyChanged(nameof(TotalAmount));
        }
    }

    public class SaleItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
}
