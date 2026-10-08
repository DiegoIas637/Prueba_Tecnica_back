using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Application.Validation;

/// <summary>
/// Valida los únicos datos personales admitidos: nombre y email del
/// comprador. No se solicitan ni validan otros datos.
/// </summary>
public static partial class BuyerValidator
{
    private const int MaxNameLength = 120;
    private const int MaxEmailLength = 200;

    public static void Validate(string? buyerName, string? buyerEmail)
    {
        if (string.IsNullOrWhiteSpace(buyerName))
            throw new InvalidSaleDataException("El nombre del comprador es obligatorio.");

        if (buyerName.Trim().Length > MaxNameLength)
            throw new InvalidSaleDataException($"El nombre del comprador no puede exceder {MaxNameLength} caracteres.");

        if (string.IsNullOrWhiteSpace(buyerEmail))
            throw new InvalidSaleDataException("El email del comprador es obligatorio.");

        if (buyerEmail.Trim().Length > MaxEmailLength)
            throw new InvalidSaleDataException($"El email del comprador no puede exceder {MaxEmailLength} caracteres.");

        if (!EmailRegex().IsMatch(buyerEmail.Trim()))
            throw new InvalidSaleDataException("El email del comprador no tiene un formato válido.");
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
