using Application.Abstractions;
using Application.Services;
using Domain.Entities;
using Infrastructure.Concurrency;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

/// <summary>
/// Construye un SaleService sobre EF Core InMemory para probar las reglas
/// de negocio R3. La concurrencia real (rowversion) la garantiza SQL Server
/// en ejecución; aquí validamos idempotencia, conflicto de referencia,
/// agotamiento de cupos y ausencia de efectos en los rechazos.
/// </summary>
public sealed class TestHarness : IAsyncDisposable
{
    public AppDbContext Context { get; }
    public SaleService SaleService { get; }

    private TestHarness(AppDbContext context, SaleService saleService)
    {
        Context = context;
        SaleService = saleService;
    }

    public static async Task<TestHarness> CreateAsync(
        int esencialQuota = 1,
        int plusQuota = 5)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"sales-tests-{Guid.NewGuid()}")
            .Options;

        // El provider InMemory no genera rowversion como SQL Server; usamos
        // un contexto de prueba que no exige esa columna. La concurrencia real
        // (rowversion) se valida contra SQL Server en ejecución.
        var context = new TestAppDbContext(options);

        context.Plans.AddRange(
            new Plan("ACC_ESENCIAL", "Accidente Esencial", 50_000m, esencialQuota),
            new Plan("ACC_PLUS", "Accidente Plus", 90_000m, plusQuota));
        await context.SaveChangesAsync();

        var saleRepo = new SaleRepository(context);
        var planRepo = new PlanRepository(context);
        var uow = new UnitOfWork(context);
        ISaleRegistrationLock lockService = new InMemorySaleRegistrationLock();

        var service = new SaleService(saleRepo, planRepo, uow, lockService);
        return new TestHarness(context, service);
    }

    public async Task<int> QuotaOf(string code)
    {
        var plan = await Context.Plans.AsNoTracking().FirstAsync(p => p.Code == code);
        return plan.AvailableQuota;
    }

    public ValueTask DisposeAsync() => Context.DisposeAsync();
}
