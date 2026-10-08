using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

/// <summary>
/// Variante de AppDbContext para pruebas con el provider InMemory, que no
/// soporta la generación automática de rowversion. Mantiene el resto del
/// modelo (índices, claves) idéntico y solo relaja la columna RowVersion.
/// </summary>
public sealed class TestAppDbContext : AppDbContext
{
    public TestAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // En InMemory la rowversion no se genera; la hacemos opcional para
        // que las pruebas de negocio puedan persistir planes.
        modelBuilder.Entity<Plan>()
            .Property(p => p.RowVersion)
            .IsRequired(false)
            .ValueGeneratedNever();
    }
}
