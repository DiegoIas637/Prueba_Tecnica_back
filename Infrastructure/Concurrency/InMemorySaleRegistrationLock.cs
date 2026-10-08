using System.Collections.Concurrent;
using Application.Abstractions;

namespace Infrastructure.Concurrency;

/// <summary>
/// Lock por clave (asesor + referencia) basado en semáforos en memoria.
/// Serializa reintentos idénticos simultáneos dentro de la instancia.
///
/// Límite documentado: válido para una única instancia. Con varias
/// instancias se necesitaría un lock distribuido; la unicidad de base de
/// datos sobre (AdvisorId, RequestReference) sigue garantizando una sola
/// venta aun sin este lock.
/// </summary>
public sealed class InMemorySaleRegistrationLock : ISaleRegistrationLock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public async Task<IDisposable> AcquireAsync(string key, CancellationToken cancellationToken = default)
    {
        var semaphore = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new Releaser(semaphore);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _semaphore.Release();
        }
    }
}
