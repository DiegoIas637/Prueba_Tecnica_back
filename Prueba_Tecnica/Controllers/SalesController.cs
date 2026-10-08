using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Prueba_Tecnica.Identity;

namespace Prueba_Tecnica.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly IAdvisorContext _advisor;

    public SalesController(ISaleService saleService, IAdvisorContext advisor)
    {
        _saleService = saleService;
        _advisor = advisor;
    }

    /// <summary>
    /// R2 + R3. Registra una venta para el asesor activo. Idempotente por
    /// (asesor, referencia): reenviar la misma solicitud devuelve la venta
    /// previa sin consumir cupo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SaleDto>> RegisterSale(
        [FromBody] RegisterSaleRequest request,
        CancellationToken cancellationToken)
    {
        var sale = await _saleService.RegisterSaleAsync(_advisor.AdvisorId, request, cancellationToken);
        return CreatedAtAction(nameof(RegisterSale), new { id = sale.Id }, sale);
    }

    /// <summary>
    /// R4. Lista paginada de ventas del asesor activo, más recientes primero.
    /// El asesor se toma del backend; no se acepta por parámetro.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SaleListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SaleListItemDto>>> GetOwnSales(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _saleService.GetOwnSalesAsync(_advisor.AdvisorId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
