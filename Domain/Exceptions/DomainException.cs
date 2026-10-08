namespace Domain.Exceptions;

/// <summary>
/// Error de negocio esperado. Se traduce a una respuesta HTTP clara,
/// nunca a un 500.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>Código estable para que el cliente discrimine el caso.</summary>
    public abstract string Code { get; }

    protected DomainException(string message) : base(message) { }
}

/// <summary>No quedan cupos disponibles para el plan solicitado.</summary>
public sealed class NoQuotaAvailableException : DomainException
{
    public override string Code => "NO_QUOTA_AVAILABLE";

    public NoQuotaAvailableException(string planName)
        : base($"No hay cupos disponibles para el plan '{planName}'.") { }
}

/// <summary>
/// La referencia ya fue aceptada con datos de negocio distintos.
/// Se conserva la venta original y se rechaza el cambio.
/// </summary>
public sealed class ReferenceConflictException : DomainException
{
    public override string Code => "REFERENCE_CONFLICT";

    public ReferenceConflictException(string reference)
        : base($"La referencia '{reference}' ya fue aceptada con otros datos de negocio.") { }
}

/// <summary>El plan solicitado no existe.</summary>
public sealed class PlanNotFoundException : DomainException
{
    public override string Code => "PLAN_NOT_FOUND";

    public PlanNotFoundException(string planCode)
        : base($"No existe el plan '{planCode}'.") { }
}

/// <summary>Datos de entrada inválidos (nombre o email).</summary>
public sealed class InvalidSaleDataException : DomainException
{
    public override string Code => "INVALID_SALE_DATA";

    public InvalidSaleDataException(string message) : base(message) { }
}
