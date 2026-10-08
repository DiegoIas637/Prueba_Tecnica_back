using Application.DTOs;
using Application.Mapping;
using Domain.Repositories;

namespace Application.Services;

public sealed class PlanService : IPlanService
{
    private readonly IPlanRepository _planRepository;

    public PlanService(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<IReadOnlyList<PlanDto>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _planRepository.GetAllAsync(cancellationToken);
        return plans.Select(p => p.ToDto()).ToList();
    }
}
