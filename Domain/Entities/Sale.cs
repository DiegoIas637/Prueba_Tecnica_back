namespace Domain.Entities;

/// <summary>
/// Registro de una venta aceptada. No es una póliza emitida, solo el
/// resultado de una solicitud aceptada por un asesor.
/// </summary>
public class Sale
{
    public Guid Id { get; private set; }

    /// <summary>Asesor dueño de la venta (A o B).</summary>
    public string AdvisorId { get; private set; } = default!;

    /// <summary>
    /// Referencia de solicitud. Única dentro del asesor (A y B pueden
    /// reutilizar el mismo texto para ventas distintas).
    /// </summary>
    public string RequestReference { get; private set; } = default!;

    public int PlanId { get; private set; }
    public string PlanCode { get; private set; } = default!;
    public string PlanName { get; private set; } = default!;

    public string BuyerName { get; private set; } = default!;
    public string BuyerEmail { get; private set; } = default!;

    /// <summary>Prima aplicada, resuelta desde el plan al momento de aceptar.</summary>
    public decimal Premium { get; private set; }

    public DateTimeOffset AcceptedAt { get; private set; }

    /// <summary>
    /// true cuando es una venta de historial precargado (no descuenta cupos
    /// de la campaña activa).
    /// </summary>
    public bool IsHistorical { get; private set; }

    private Sale() { }

    public Sale(
        string advisorId,
        string requestReference,
        Plan plan,
        string buyerName,
        string buyerEmail,
        bool isHistorical = false)
    {
        Id = Guid.NewGuid();
        AdvisorId = advisorId;
        RequestReference = requestReference;
        PlanId = plan.Id;
        PlanCode = plan.Code;
        PlanName = plan.Name;
        BuyerName = buyerName;
        BuyerEmail = buyerEmail;
        Premium = plan.Premium;
        AcceptedAt = DateTimeOffset.UtcNow;
        IsHistorical = isHistorical;
    }

    /// <summary>
    /// Ajusta la fecha de aceptación. Uso exclusivo para la carga de
    /// historial reproducible; no debe usarse en ventas de campaña.
    /// </summary>
    public void BackdateForHistory(DateTimeOffset acceptedAt)
    {
        if (!IsHistorical)
            throw new InvalidOperationException("Solo las ventas de historial pueden ajustar su fecha.");

        AcceptedAt = acceptedAt;
    }

    /// <summary>
    /// Compara los datos de negocio de un nuevo intento con esta venta.
    /// Si difieren, el reenvío de la misma referencia debe rechazarse (R3).
    /// </summary>
    public bool MatchesBusinessData(string planCode, string buyerName, string buyerEmail)
        => PlanCode == planCode
           && string.Equals(BuyerName, buyerName, StringComparison.Ordinal)
           && string.Equals(BuyerEmail, buyerEmail, StringComparison.OrdinalIgnoreCase);
}
