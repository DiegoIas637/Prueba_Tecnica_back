using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Prueba_Tecnica.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PlansController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    /// <summary>R1. Consultar planes: nombre, prima y disponibilidad actual.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanDto>>> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await _planService.GetPlansAsync(cancellationToken);
        return Ok(plans);
    }
}
