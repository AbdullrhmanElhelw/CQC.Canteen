using CQC.Canteen.Domain.Entities;

namespace CQC.Canteen.BusinessLogic.DTOs.Products;

public class CreateProductDto
{
    public string Name { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }

    public static implicit operator Product(CreateProductDto createDto)
    {
        return new Product
        {
            Name = createDto.Name,
            PurchasePrice = createDto.PurchasePrice,
            SalePrice = createDto.SalePrice,
            StockQuantity = createDto.StockQuantity,
            CategoryId = createDto.CategoryId,
            IsActive = true,
        };
    }
}
