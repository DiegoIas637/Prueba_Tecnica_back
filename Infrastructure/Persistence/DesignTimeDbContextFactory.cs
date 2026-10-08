using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

/// <summary>
/// Fábrica usada solo por las herramientas de EF Core en tiempo de diseño
/// (dotnet ef migrations / database update). No requiere una cadena real:
/// las migraciones se generan a partir del modelo. En ejecución, la cadena
/// se toma de appsettings.json (ConnectionStrings:DefaultConnection).
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=.;Database=SeguraVida;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AppDbContext(options);
    }
}
