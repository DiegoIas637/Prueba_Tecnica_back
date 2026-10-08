using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Application.Validation;

/// <summary>
/// Valida el formato de la referencia de solicitud.
///
/// Formato documentado: cadena de 6 a 40 caracteres compuesta por letras
/// mayúsculas/minúsculas, dígitos, guion y guion bajo (ej. "REF-A-0001").
/// La unicidad se evalúa dentro del asesor, por lo que A y B pueden usar
/// el mismo texto para ventas distintas.
/// </summary>
public static partial class ReferenceValidator
{
    public static void Validate(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new InvalidSaleDataException("La referencia de solicitud es obligatoria.");

        if (!ReferenceRegex().IsMatch(reference.Trim()))
            throw new InvalidSaleDataException(
                "La referencia debe tener entre 6 y 40 caracteres y solo letras, dígitos, '-' o '_'.");
    }

    [GeneratedRegex(@"^[A-Za-z0-9_-]{6,40}$", RegexOptions.CultureInvariant)]
    private static partial Regex ReferenceRegex();
}
