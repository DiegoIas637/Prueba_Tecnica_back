namespace Application.DTOs;

/// <summary>
/// Proyección mínima para el listado de ventas propias (R4).
/// Solo expone los campos necesarios para esa vista.
/// </summary>
public record SaleListItemDto(
    Guid Id,
    string RequestReference,
    string PlanName,
    decimal Premium,
    string BuyerName,
    DateTimeOffset AcceptedAt);
