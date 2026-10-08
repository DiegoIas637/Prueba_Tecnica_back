using Domain.Entities;

namespace Domain.Repositories;

public interface IPlanRepository
{
    Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
