using CQC.Canteen.BusinessLogic.DTOs.Products;
using FluentResults;
namespace CQC.Canteen.BusinessLogic.Services.Products;

public interface IProductService
{
    Task<Result<List<ProductDto>>> GetAllProductsAsync(CancellationToken token);

    // (1) إضافة الميثود الجديدة
    Task<Result<ProductDto>> AddNewProductAsync(CreateProductDto createDto, CancellationToken token);

    // (1) ميثود لجلب بيانات الصنف للتعديل
    Task<Result<ProductDetailsDto>> GetProductDetailsByIdAsync(int id, CancellationToken token);

    // (2) ميثود لحفظ التعديلات
    // (بترجع الصنف بشكله في الجدول عشان نحدث الجدول)
    Task<Result<ProductDto>> UpdateProductAsync(ProductDetailsDto updateDto, CancellationToken token);
}

