using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly AppDbContext _context;

    public SaleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Sale?> GetByAdvisorAndReferenceAsync(
        string advisorId,
        string requestReference,
        CancellationToken cancellationToken = default)
        => await _context.Sales
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.AdvisorId == advisorId && s.RequestReference == requestReference,
                cancellationToken);

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
        => await _context.Sales.AddAsync(sale, cancellationToken);

    public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> GetByAdvisorPagedAsync(
        string advisorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .AsNoTracking()
            .Where(s => s.AdvisorId == advisorId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.AcceptedAt)
            .ThenByDescending(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
