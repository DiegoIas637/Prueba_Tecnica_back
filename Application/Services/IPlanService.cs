using Application.DTOs;

namespace Application.Services;

public interface IPlanService
{
    Task<IReadOnlyList<PlanDto>> GetPlansAsync(CancellationToken cancellationToken = default);
}
