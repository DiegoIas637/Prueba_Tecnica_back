using Application.Abstractions;
using Domain.Repositories;
using Infrastructure.Concurrency;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // El lock de registro es por instancia -> singleton.
        services.AddSingleton<ISaleRegistrationLock, InMemorySaleRegistrationLock>();

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
