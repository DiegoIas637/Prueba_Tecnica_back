namespace Application.Abstractions;

/// <summary>
/// Se lanza cuando la persistencia detecta una escritura concurrente sobre
/// el mismo recurso (p. ej. dos asesores por el último cupo). Es una
/// abstracción de infraestructura para no acoplar la capa Application a EF.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception? inner = null)
        : base(message, inner) { }
}
