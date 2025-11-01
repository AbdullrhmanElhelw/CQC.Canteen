using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.Domain.Data;
using CQC.Canteen.Domain.Entities;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CQC.Canteen.BusinessLogic.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly CanteenDbContext _context;
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<CategoryDetailsDto> _updateValidator;

    public CategoryService(
        CanteenDbContext context,
        IValidator<CreateCategoryDto> createValidator,
        IValidator<CategoryDetailsDto> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<List<CategoryDto>>> GetAllCategoriesAsync(CancellationToken token)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(token);

        return Result.Ok(categories);
    }

    public async Task<Result<CategoryDto>> AddNewCategoryAsync(CreateCategoryDto createDto, CancellationToken token)
    {
        // الخطوة 1: التحقق من صحة البيانات
        var validationResult = await _createValidator.ValidateAsync(createDto, token);
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        // الخطوة 2: التأكد من أن الاسم غير مكرر
        var nameExists = await _context.Categories
            .AnyAsync(c => c.Name == createDto.Name, token);
        if (nameExists)
        {
            return Result.Fail("اسم الفئة موجود بالفعل.");
        }

        // الخطوة 3: إنشاء الفئة الجديدة
        var newCategory = new Category
        {
            Name = createDto.Name
        };

        // الخطوة 4: الإضافة والحفظ
        await _context.Categories.AddAsync(newCategory, token);
        await _context.SaveChangesAsync(token);

        // الخطوة 5: إرجاع الفئة الجديدة بشكلها الـ DTO
        var resultDto = new CategoryDto
        {
            Id = newCategory.Id,
            Name = newCategory.Name
        };

        return Result.Ok(resultDto);
    }

    public async Task<Result<CategoryDetailsDto>> GetCategoryDetailsByIdAsync(int id, CancellationToken token)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDetailsDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .FirstOrDefaultAsync(token);

        if (category is null)
        {
            return Result.Fail("لم يتم العثور على الفئة.");
        }

        return Result.Ok(category);
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(CategoryDetailsDto updateDto, CancellationToken token)
    {
        // الخطوة 1: التحقق من صحة البيانات
        var validationResult = await _updateValidator.ValidateAsync(updateDto, token);
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        // الخطوة 2: التأكد من أن الفئة موجودة
        var categoryToUpdate = await _context.Categories.FindAsync(updateDto.Id);
        if (categoryToUpdate is null)
        {
            return Result.Fail("لم يتم العثور على الفئة المراد تعديلها.");
        }

        // الخطوة 3: التأكد من أن الاسم الجديد غير مكرر (لفئة أخرى)
        var nameExists = await _context.Categories
            .AnyAsync(c => c.Name == updateDto.Name && c.Id != updateDto.Id, token);
        if (nameExists)
        {
            return Result.Fail("اسم الفئة مستخدم لفئة أخرى.");
        }

        // الخطوة 4: تحديث البيانات
        categoryToUpdate.Name = updateDto.Name;

        // الخطوة 5: الحفظ
        await _context.SaveChangesAsync(token);

        // الخطوة 6: إرجاع الفئة بشكلها الـ DTO
        var resultDto = new CategoryDto
        {
            Id = categoryToUpdate.Id,
            Name = categoryToUpdate.Name
        };

        return Result.Ok(resultDto);
    }
}