using Application.DTOs;

namespace Application.Services;

public interface ISaleService
{
    /// <summary>Registra una venta aplicando idempotencia y control de cupos (R2, R3).</summary>
    Task<SaleDto> RegisterSaleAsync(
        string advisorId,
        RegisterSaleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Lista paginada de ventas del asesor activo (R4).</summary>
    Task<PagedResult<SaleListItemDto>> GetOwnSalesAsync(
        string advisorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
