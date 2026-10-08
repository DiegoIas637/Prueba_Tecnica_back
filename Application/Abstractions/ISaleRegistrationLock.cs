namespace Application.Abstractions;

/// <summary>
/// Serializa los intentos concurrentes que comparten la misma clave de
/// idempotencia (asesor + referencia). Garantiza que dos envíos idénticos
/// simultáneos produzcan una sola venta (R3).
/// </summary>
public interface ISaleRegistrationLock
{
    Task<IDisposable> AcquireAsync(string key, CancellationToken cancellationToken = default);
}
