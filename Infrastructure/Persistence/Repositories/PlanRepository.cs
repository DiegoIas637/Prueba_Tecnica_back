using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PlanRepository : IPlanRepository
{
    private readonly AppDbContext _context;

    public PlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Plans
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public async Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _context.Plans
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
}
