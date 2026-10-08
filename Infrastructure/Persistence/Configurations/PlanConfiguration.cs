using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Code).HasMaxLength(40).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Premium).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.AvailableQuota).IsRequired();

        // Token de concurrencia optimista para el control de cupos (R3).
        builder.Property(p => p.RowVersion).IsRowVersion();
    }
}
