using CQC.Canteen.BusinessLogic.DTOs.Categories;
using FluentResults;

namespace CQC.Canteen.BusinessLogic.Services.Categories;

public interface ICategoryService
{
    Task<Result<List<CategoryDto>>> GetAllCategoriesAsync(CancellationToken token);
}
