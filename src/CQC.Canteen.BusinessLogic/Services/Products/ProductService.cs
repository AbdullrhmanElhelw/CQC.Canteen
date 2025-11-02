using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.Domain.Data;
using CQC.Canteen.Domain.Entities;
using FluentResults;
using FluentValidation; // <-- (1) إضافة
using Microsoft.EntityFrameworkCore;

namespace CQC.Canteen.BusinessLogic.Services.Products;

public class ProductService : IProductService
{
    private readonly CanteenDbContext _context;
    // (2) تعديل: هنستخدم DI للـ Validators
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<ProductDetailsDto> _updateValidator;

    // (3) تعديل الـ Constructor
    public ProductService(
        CanteenDbContext context,
        IValidator<CreateProductDto> createValidator,
        IValidator<ProductDetailsDto> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<List<ProductDto>>> GetAllProductsAsync(CancellationToken token)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SalePrice = p.SalePrice,
                PurchasePrice = p.PurchasePrice, // تأكد من إضافة هذا السطر
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name,
                IsActive = p.IsActive
            })
            .ToListAsync(token);

        return Result.Ok(products);
    }

    public async Task<Result<ProductDto>> AddNewProductAsync(CreateProductDto createDto, CancellationToken token)
    {
        // الخطوة 1: التحقق من صحة البيانات
        // (4) تعديل: استخدام الـ Validator المحقون
        var validationResult = await _createValidator.ValidateAsync(createDto, token);
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        // الخطوة 2: التأكد إن الاسم مش مكرر
        var nameExists = await _context.Products
            .AnyAsync(p => p.Name == createDto.Name, token);
        if (nameExists)
        {
            return Result.Fail("اسم الصنف ده موجود قبل كده.");
        }

        // الخطوة 3: التأكد إن الفئة موجودة
        var category = await _context.Categories.FindAsync(createDto.CategoryId);
        if (category is null)
        {
            return Result.Fail("الفئة المختارة غير موجودة.");
        }

        // الخطوة 4: إنشاء الصنف الجديد
        var newProduct = new Product
        {
            Name = createDto.Name,
            PurchasePrice = createDto.PurchasePrice,
            SalePrice = createDto.SalePrice,
            StockQuantity = createDto.StockQuantity,
            CategoryId = createDto.CategoryId,
            IsActive = true,
            // (البيانات الأساسية زي CreatedDate هتتضاف لو AuditableEntity بيعملها)
        };

        // الخطوة 5: الإضافة والحفظ
        await _context.Products.AddAsync(newProduct, token);
        await _context.SaveChangesAsync(token);

        // الخطوة 6: إرجاع الصنف الجديد بشكله الـ DTO
        var resultDto = new ProductDto
        {
            Id = newProduct.Id,
            Name = newProduct.Name,
            SalePrice = newProduct.SalePrice,
            StockQuantity = newProduct.StockQuantity,
            CategoryName = category.Name,
            IsActive = newProduct.IsActive
        };

        return Result.Ok(resultDto);
    }

    // (5) إضافة الميثودز الجديدة

    // ميثود لجلب بيانات الصنف كاملة
    public async Task<Result<ProductDetailsDto>> GetProductDetailsByIdAsync(int id, CancellationToken token)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDetailsDto
            {
                Id = p.Id,
                Name = p.Name,
                PurchasePrice = p.PurchasePrice,
                SalePrice = p.SalePrice,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync(token);

        if (product is null)
        {
            return Result.Fail("لم يتم العثور على الصنف.");
        }

        return Result.Ok(product);
    }

    // ميثود لحفظ التعديلات
    public async Task<Result<ProductDto>> UpdateProductAsync(ProductDetailsDto updateDto, CancellationToken token)
    {
        // الخطوة 1: التحقق من صحة البيانات
        var validationResult = await _updateValidator.ValidateAsync(updateDto, token);
        if (!validationResult.IsValid)
        {
            return Result.Fail(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        // الخطوة 2: التأكد إن الصنف موجود
        var productToUpdate = await _context.Products.FindAsync(updateDto.Id);
        if (productToUpdate is null)
        {
            return Result.Fail("لم يتم العثور على الصنف المراد تعديله.");
        }

        // الخطوة 3: التأكد إن الاسم الجديد مش مكرر (لصنف تاني)
        var nameExists = await _context.Products
            .AnyAsync(p => p.Name == updateDto.Name && p.Id != updateDto.Id, token);
        if (nameExists)
        {
            return Result.Fail("اسم الصنف ده مستخدم لصنف آخر.");
        }

        // الخطوة 4: التأكد إن الفئة موجودة
        var category = await _context.Categories.FindAsync(updateDto.CategoryId);
        if (category is null)
        {
            return Result.Fail("الفئة المختارة غير موجودة.");
        }

        // الخطوة 5: تحديث البيانات
        productToUpdate.Name = updateDto.Name;
        productToUpdate.PurchasePrice = updateDto.PurchasePrice;
        productToUpdate.SalePrice = updateDto.SalePrice;
        productToUpdate.StockQuantity = updateDto.StockQuantity;
        productToUpdate.CategoryId = updateDto.CategoryId;
        productToUpdate.IsActive = updateDto.IsActive;

        // الخطوة 6: الحفظ
        await _context.SaveChangesAsync(token);

        // الخطوة 7: إرجاع الصنف بشكله الـ DTO (اللي بيتعرض في الجدول)
        var resultDto = new ProductDto
        {
            Id = productToUpdate.Id,
            Name = productToUpdate.Name,
            SalePrice = productToUpdate.SalePrice,
            StockQuantity = productToUpdate.StockQuantity,
            CategoryName = category.Name,
            IsActive = productToUpdate.IsActive
        };

        return Result.Ok(resultDto);
    }



}

