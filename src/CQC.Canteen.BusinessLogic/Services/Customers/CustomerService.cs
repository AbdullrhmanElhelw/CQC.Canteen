using CQC.Canteen.BusinessLogic.DTOs.Customers;
using CQC.Canteen.Domain.Data;
using CQC.Canteen.Domain.Entities;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CQC.Canteen.BusinessLogic.Services.Customers;

public class CustomerService : ICustomerService
{
    private readonly CanteenDbContext _context;
    private readonly IValidator<CreateCustomerDto> _createValidator;
    private readonly IValidator<CustomerDetailsDto> _updateValidator;

    public CustomerService(
        CanteenDbContext context,
        IValidator<CreateCustomerDto> createValidator,
        IValidator<CustomerDetailsDto> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // جلب كل العملاء
    public async Task<Result<List<CustomerDto>>> GetAllCustomersAsync(CancellationToken token)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                CurrentBalance = c.CurrentBalance,
                IsActive = c.IsActive,
                IsMilitary = c.IsMilitary,
                Rank = c.Rank
            })
            .ToListAsync(token);

        return Result.Ok(customers);
    }

    // إضافة عميل جديد
    public async Task<Result<CustomerDto>> AddCustomerAsync(CreateCustomerDto dto, CancellationToken token)
    {
        var validation = await _createValidator.ValidateAsync(dto, token);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        // تأكد إن الاسم مش مكرر
        bool exists = await _context.Customers.AnyAsync(c => c.Name == dto.Name, token);
        if (exists)
            return Result.Fail("اسم العميل موجود بالفعل.");

        var entity = new Customer
        {
            Name = dto.Name,
            IsMilitary = dto.IsMilitary,
            Rank = dto.IsMilitary ? dto.Rank : null,
            CurrentBalance = 0,
            IsActive = true
        };

        await _context.Customers.AddAsync(entity, token);
        await _context.SaveChangesAsync(token);

        var resultDto = new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CurrentBalance = entity.CurrentBalance,
            IsMilitary = entity.IsMilitary,
            Rank = entity.Rank,
            IsActive = entity.IsActive
        };

        return Result.Ok(resultDto);
    }

    // جلب بيانات عميل واحد
    public async Task<Result<CustomerDetailsDto>> GetCustomerDetailsByIdAsync(int id, CancellationToken token)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerDetailsDto
            {
                Id = c.Id,
                Name = c.Name,
                CurrentBalance = c.CurrentBalance,
                IsActive = c.IsActive,
                IsMilitary = c.IsMilitary,
                Rank = c.Rank
            })
            .FirstOrDefaultAsync(token);

        if (customer == null)
            return Result.Fail("لم يتم العثور على العميل.");

        return Result.Ok(customer);
    }

    // تحديث بيانات العميل
    public async Task<Result<CustomerDto>> UpdateCustomerAsync(CustomerDetailsDto dto, CancellationToken token)
    {
        var validation = await _updateValidator.ValidateAsync(dto, token);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        var entity = await _context.Customers.FindAsync(dto.Id);
        if (entity == null)
            return Result.Fail("العميل غير موجود.");

        bool nameTaken = await _context.Customers
            .AnyAsync(c => c.Name == dto.Name && c.Id != dto.Id, token);
        if (nameTaken)
            return Result.Fail("اسم العميل مستخدم بالفعل.");

        entity.Name = dto.Name;
        entity.IsActive = dto.IsActive;
        entity.IsMilitary = dto.IsMilitary;
        entity.Rank = dto.IsMilitary ? dto.Rank : null;

        await _context.SaveChangesAsync(token);

        var result = new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CurrentBalance = entity.CurrentBalance,
            IsMilitary = entity.IsMilitary,
            Rank = entity.Rank,
            IsActive = entity.IsActive
        };

        return Result.Ok(result);
    }

    // تسوية الرصيد (تصفير)
    public async Task<Result> SettleCustomerBalanceAsync(int id, CancellationToken token)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return Result.Fail("العميل غير موجود.");

        customer.CurrentBalance = 0;
        await _context.SaveChangesAsync(token);

        return Result.Ok();
    }
}
