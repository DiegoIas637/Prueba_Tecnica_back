using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.AdvisorId).HasMaxLength(10).IsRequired();
        builder.Property(s => s.RequestReference).HasMaxLength(40).IsRequired();

        builder.Property(s => s.PlanCode).HasMaxLength(40).IsRequired();
        builder.Property(s => s.PlanName).HasMaxLength(120).IsRequired();

        builder.Property(s => s.BuyerName).HasMaxLength(120).IsRequired();
        builder.Property(s => s.BuyerEmail).HasMaxLength(200).IsRequired();

        builder.Property(s => s.Premium).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(s => s.AcceptedAt).IsRequired();
        builder.Property(s => s.IsHistorical).IsRequired();

        // Unicidad de la referencia DENTRO del asesor (A y B pueden repetir texto).
        // Esto refuerza la idempotencia a nivel de base de datos.
        builder.HasIndex(s => new { s.AdvisorId, s.RequestReference }).IsUnique();

        // Índice para el listado paginado por asesor, más recientes primero (R4).
        builder.HasIndex(s => new { s.AdvisorId, s.AcceptedAt });

        builder.Property(s => s.PlanId).IsRequired();
    }
}
