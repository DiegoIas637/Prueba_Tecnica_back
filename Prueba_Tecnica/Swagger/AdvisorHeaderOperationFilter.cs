using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Prueba_Tecnica.Identity;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Prueba_Tecnica.Swagger;

/// <summary>
/// Añade el header "X-Advisor-Id" como parámetro en Swagger para los
/// endpoints que lo requieren, de modo que se pueda probar como asesor
/// A o B directamente desde la UI. No se aplica a /api/plans (público).
/// </summary>
public sealed class AdvisorHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controller = context.MethodInfo.DeclaringType?.Name ?? string.Empty;

        // Los endpoints de planes no necesitan asesor.
        if (controller.Contains("Plans", StringComparison.OrdinalIgnoreCase))
            return;

        operation.Parameters ??= new List<IOpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = AdvisorContext.HeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Asesor activo (simulación de identidad): A o B.",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Enum = new List<JsonNode> { "A", "B" },
                Default = "A"
            }
        });
    }
}
