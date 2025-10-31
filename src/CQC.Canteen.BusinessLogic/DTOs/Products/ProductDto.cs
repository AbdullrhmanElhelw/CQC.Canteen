namespace CQC.Canteen.BusinessLogic.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal SalePrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; }
    public bool IsActive { get; set; }
}