using CQC.Canteen.BusinessLogic.Services.Authentication;
using CQC.Canteen.BusinessLogic.Services.Categories;
using CQC.Canteen.BusinessLogic.Services.Customers;
using CQC.Canteen.BusinessLogic.Services.Orders;
using CQC.Canteen.BusinessLogic.Services.Products;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CQC.Canteen.BusinessLogic;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddBusniessLogic(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();



        services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(AuthenticationService))!);

        return services;
    }
}