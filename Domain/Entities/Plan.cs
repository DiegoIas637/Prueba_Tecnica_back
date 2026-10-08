namespace Domain.Entities;

/// <summary>
/// Representa un plan de seguro de la campaña. La prima y la disponibilidad
/// de cupos son autoridad del backend; nunca se confía en valores del cliente.
/// </summary>
public class Plan
{
    public int Id { get; private set; }

    /// <summary>Código estable e inmutable del plan (ej. ACC_ESENCIAL).</summary>
    public string Code { get; private set; } = default!;

    /// <summary>Nombre visible del plan.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Prima por venta en COP, resuelta siempre desde el plan.</summary>
    public decimal Premium { get; private set; }

    /// <summary>Cupos disponibles actualmente. Nunca puede ser negativo.</summary>
    public int AvailableQuota { get; private set; }

    /// <summary>
    /// Token de concurrencia optimista. EF lo usa para detectar escrituras
    /// simultáneas sobre el mismo plan (dos asesores por el último cupo).
    /// </summary>
    public byte[] RowVersion { get; private set; } = default!;

    private Plan() { }

    public Plan(string code, string name, decimal premium, int initialQuota)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código del plan es obligatorio.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del plan es obligatorio.", nameof(name));
        if (premium <= 0)
            throw new ArgumentException("La prima debe ser mayor que cero.", nameof(premium));
        if (initialQuota < 0)
            throw new ArgumentException("Los cupos iniciales no pueden ser negativos.", nameof(initialQuota));

        Code = code;
        Name = name;
        Premium = premium;
        AvailableQuota = initialQuota;
    }

    /// <summary>Indica si queda al menos un cupo disponible.</summary>
    public bool HasQuota => AvailableQuota > 0;

    /// <summary>
    /// Descuenta un cupo de forma segura. Garantiza que los cupos nunca
    /// queden negativos (regla R3).
    /// </summary>
    public void ConsumeOneQuota()
    {
        if (AvailableQuota <= 0)
            throw new InvalidOperationException("No hay cupos disponibles para este plan.");

        AvailableQuota -= 1;
    }
}
