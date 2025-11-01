using CQC.Canteen.BusinessLogic.DTOs.Categories;
using FluentResults;

namespace CQC.Canteen.BusinessLogic.Services.Categories;

public interface ICategoryService
{
    Task<Result<List<CategoryDto>>> GetAllCategoriesAsync(CancellationToken token);

    // ميثود لإضافة فئة جديدة
    Task<Result<CategoryDto>> AddNewCategoryAsync(CreateCategoryDto createDto, CancellationToken token);

    // ميثود لجلب بيانات الفئة للتعديل
    Task<Result<CategoryDetailsDto>> GetCategoryDetailsByIdAsync(int id, CancellationToken token);

    // ميثود لحفظ التعديلات
    Task<Result<CategoryDto>> UpdateCategoryAsync(CategoryDetailsDto updateDto, CancellationToken token);
}