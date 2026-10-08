namespace Application.DTOs;

/// <summary>
/// Solicitud de registro de venta (R2). La prima no se recibe del cliente:
/// se resuelve en el backend desde el plan.
/// </summary>
public record RegisterSaleRequest(
    string PlanCode,
    string BuyerName,
    string BuyerEmail,
    string RequestReference);
