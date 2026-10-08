namespace Prueba_Tecnica.Identity;

/// <summary>
/// Identidad simulada del asesor activo.
///
/// Simulación: el asesor se establece con el header HTTP "X-Advisor-Id"
/// (valores admitidos: "A" o "B"). No hay login real; esto evita construir
/// autenticación pero permite comprobar ambos contextos cambiando el header.
///
/// Límite: cualquiera que conozca el header puede suplantar a un asesor.
/// La validación de acceso a ventas (R4) se hace SIEMPRE en el backend
/// filtrando por este asesor; no se aceptan ids de asesor por query/body.
/// </summary>
public interface IAdvisorContext
{
    string AdvisorId { get; }
}

public sealed class AdvisorContext : IAdvisorContext
{
    public const string HeaderName = "X-Advisor-Id";
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { "A", "B" };

    public string AdvisorId { get; }

    public AdvisorContext(IHttpContextAccessor accessor)
    {
        var raw = accessor.HttpContext?.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(raw) || !Allowed.Contains(raw.Trim()))
            throw new UnauthorizedAdvisorException();

        // Normalizamos a mayúscula para una clave de asesor estable.
        AdvisorId = raw.Trim().ToUpperInvariant();
    }
}

/// <summary>Se lanza cuando no se identifica un asesor válido (A o B).</summary>
public sealed class UnauthorizedAdvisorException : Exception
{
    public UnauthorizedAdvisorException()
        : base($"Debe indicar un asesor válido en el header '{AdvisorContext.HeaderName}' (A o B).") { }
}
