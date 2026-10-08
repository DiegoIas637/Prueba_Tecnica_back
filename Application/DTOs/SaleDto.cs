namespace Application.DTOs;

/// <summary>Resultado completo de una venta aceptada (respuesta de R2).</summary>
public record SaleDto(
    Guid Id,
    string RequestReference,
    string AdvisorId,
    string PlanCode,
    string PlanName,
    string BuyerName,
    string BuyerEmail,
    decimal Premium,
    DateTimeOffset AcceptedAt);
