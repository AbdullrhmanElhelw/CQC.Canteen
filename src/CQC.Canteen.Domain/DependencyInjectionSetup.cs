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
        string connectionStringValue = configuration.GetConnectionString(
            ConnectionString.DefaultValue);

        if (string.IsNullOrWhiteSpace(connectionStringValue))
        {
            throw new ArgumentNullException(
                nameof(ConnectionString),
                "Connection string 'DefaultConnection' was not found in configuration."
            );
        }

        var connectionString = new ConnectionString(connectionStringValue);

        services.AddSingleton(connectionString);

        services.AddDbContext<CanteenDbContext>((serviceProvider, options) =>
        {

            options.UseSqlServer(connectionString.Value, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        return services;
    }
}
