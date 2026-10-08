using Domain.Entities;

namespace Domain.Repositories;

public interface ISaleRepository
{
    /// <summary>
    /// Busca una venta por su clave de idempotencia (asesor + referencia).
    /// </summary>
    Task<Sale?> GetByAdvisorAndReferenceAsync(
        string advisorId,
        string requestReference,
        CancellationToken cancellationToken = default);

    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista paginada de ventas de un asesor, más recientes primero.
    /// El filtro por asesor es responsabilidad del backend (R4).
    /// </summary>
    Task<(IReadOnlyList<Sale> Items, int TotalCount)> GetByAdvisorPagedAsync(
        string advisorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
