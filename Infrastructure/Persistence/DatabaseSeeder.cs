using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

/// <summary>
/// Carga reproducible de datos: los dos planes de campaña y 10.000 ventas
/// de historial repartidas entre A y B. El historial NO descuenta cupos de
/// la campaña activa (se marca IsHistorical = true y usa referencias
/// propias con prefijo HIST-).
/// </summary>
public sealed class DatabaseSeeder
{
    private const int HistoricalSalesCount = 10_000;

    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedPlansAsync(cancellationToken);
        await SeedHistoricalSalesAsync(cancellationToken);
    }

    private async Task SeedPlansAsync(CancellationToken cancellationToken)
    {
        if (await _context.Plans.AnyAsync(cancellationToken))
            return;

        _context.Plans.AddRange(
            new Plan("ACC_ESENCIAL", "Accidente Esencial", 50_000m, 1),
            new Plan("ACC_PLUS", "Accidente Plus", 90_000m, 5));

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Planes de campaña sembrados.");
    }

    private async Task SeedHistoricalSalesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Sales.AnyAsync(s => s.IsHistorical, cancellationToken))
            return;

        var plans = await _context.Plans.ToListAsync(cancellationToken);
        if (plans.Count == 0) return;

        // Semilla fija para que la carga sea reproducible.
        var random = new Random(20261008);
        var advisors = new[] { "A", "B" };
        var batch = new List<Sale>(1_000);

        for (var i = 0; i < HistoricalSalesCount; i++)
        {
            var advisor = advisors[i % advisors.Length];
            var plan = plans[random.Next(plans.Count)];
            var reference = $"HIST-{advisor}-{i:D5}";

            var sale = BuildHistoricalSale(advisor, reference, plan, i, random);
            batch.Add(sale);

            if (batch.Count == 1_000)
            {
                await _context.Sales.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _context.ChangeTracker.Clear();
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await _context.Sales.AddRangeAsync(batch, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _context.ChangeTracker.Clear();
        }

        _logger.LogInformation("{Count} ventas de historial sembradas.", HistoricalSalesCount);
    }

    private static Sale BuildHistoricalSale(string advisor, string reference, Plan plan, int index, Random random)
    {
        var buyerName = $"Comprador Historico {index:D5}";
        var buyerEmail = $"comprador{index:D5}@ejemplo.test";

        var sale = new Sale(advisor, reference, plan, buyerName, buyerEmail, isHistorical: true);

        // Repartimos las fechas de aceptación en el último año para que el
        // orden "más recientes primero" sea significativo.
        var daysAgo = random.Next(1, 365);
        var acceptedAt = DateTimeOffset.UtcNow.AddDays(-daysAgo).AddMinutes(-(index % 1440));
        sale.BackdateForHistory(acceptedAt);

        return sale;
    }
}
