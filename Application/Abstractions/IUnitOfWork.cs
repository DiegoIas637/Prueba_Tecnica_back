namespace Application.Abstractions;

/// <summary>
/// Confirma los cambios acumulados en una sola transacción. Permite
/// detectar conflictos de concurrencia al persistir.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
