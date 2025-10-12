using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MutualFundScanner.Infrastructure.Data;
using MutualFundScanner.Infrastructure.UnitOfWork;
using UoW = MutualFundScanner.Infrastructure.UnitOfWork.UnitOfWork;

namespace MutualFundScanner.Infrastructure.DI;

public static class UnitOfWorkConfiguration
{
    /// <summary>
    /// Registers the Unit of Work pattern services with the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        // Register DbContext as Scoped (default Entity Framework behavior)
        // This ensures one context per HTTP request
        services.AddScoped<MutualFundContext>();
        
        // Register UnitOfWork as Scoped to match DbContext lifetime
        // This ensures the UnitOfWork and DbContext have the same scope
        services.AddScoped<IUnitOfWork, UoW>();
        
        return services;
    }
    
    /// <summary>
    /// Alternative registration method with custom DbContext configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUnitOfWorkWithDbContext(
        this IServiceCollection services, 
        string connectionString)
    {
        // Register DbContext with connection string
        services.AddDbContext<MutualFundContext>(options =>
            options.UseSqlServer(connectionString));
        
        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UoW>();
        
        return services;
    }
}
