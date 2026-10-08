using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Prueba_Tecnica.Identity;
using Prueba_Tecnica.Middleware;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "FrontendCors";

// --- Servicios ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Swagger / OpenAPI (Swashbuckle). Incluye el header X-Advisor-Id como
// parámetro global para poder probar como asesor A o B desde la UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "SeguraVida - API de venta de seguros",
        Version = "v1",
        Description = "Prototipo de registro y consulta de ventas (R1-R4)."
    });

    options.OperationFilter<Prueba_Tecnica.Swagger.AdvisorHeaderOperationFilter>();
});

builder.Services.AddHttpContextAccessor();

// Identidad simulada del asesor (header X-Advisor-Id). Scoped: una por request.
builder.Services.AddScoped<IAdvisorContext, AdvisorContext>();

// Capas de Clean Architecture.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS para el frontend React (Vite) en desarrollo.
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SeguraVida API v1");
        options.RoutePrefix = "swagger"; // disponible en /swagger
    });

    // Abre Swagger en el navegador al arrancar (también al ejecutar el .exe
    // directamente, donde no aplica launchSettings.json). Desactivable con
    // Swagger:OpenBrowserOnStartup = false.
    if (app.Configuration.GetValue("Swagger:OpenBrowserOnStartup", true))
        OpenSwaggerInBrowser(app);
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapControllers();

// Migración + carga de datos (planes + 10.000 ventas de historial).
// Controlado por configuración para no sembrar en escenarios de prueba.
if (app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (app.Configuration.GetValue("Database:SeedOnStartup", true))
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}

app.Run();

// Abre /swagger en el navegador una vez el servidor está escuchando.
static void OpenSwaggerInBrowser(WebApplication app)
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            // Preferimos la URL http para evitar fricción con el cert de dev.
            var address = app.Urls.FirstOrDefault(u => u.StartsWith("http://"))
                          ?? app.Urls.FirstOrDefault();
            if (address is null) return;

            var url = $"{address.TrimEnd('/')}/swagger";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Abrir el navegador es una comodidad; nunca debe tumbar el arranque.
        }
    });
}

// Expuesto para pruebas de integración (WebApplicationFactory).
public partial class Program { }
