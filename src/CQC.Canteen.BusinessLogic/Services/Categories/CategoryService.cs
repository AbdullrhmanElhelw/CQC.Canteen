using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.Domain.Data;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace CQC.Canteen.BusinessLogic.Services.Categories;

public class CategoryService(CanteenDbContext context) : ICategoryService
{
    public async Task<Result<List<CategoryDto>>> GetAllCategoriesAsync(CancellationToken token)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(token);

        return Result.Ok(categories);
    }
}
