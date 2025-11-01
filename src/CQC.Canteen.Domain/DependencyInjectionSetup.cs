using CQC.Canteen.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CQC.Canteen.Domain;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddDomain(
     this IServiceCollection services,
     IConfiguration configuration)
    {
        string connectionStringValue = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionStringValue))
        {
            throw new ArgumentNullException("Connection string not found");
        }

        services.AddDbContext<CanteenDbContext>(options =>
            options.UseSqlServer(connectionStringValue, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
            }));

        return services;
    }

}
