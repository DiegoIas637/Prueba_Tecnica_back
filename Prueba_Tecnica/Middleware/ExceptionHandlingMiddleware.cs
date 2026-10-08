using System.Text.Json;
using Application.Abstractions;
using Domain.Exceptions;
using Prueba_Tecnica.Identity;

namespace Prueba_Tecnica.Middleware;

/// <summary>
/// Traduce excepciones a respuestas HTTP claras. Los errores de negocio
/// nunca se convierten en 500; cada caso trae un código estable para el
/// cliente. No se exponen datos personales del comprador.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAdvisorException ex)
        {
            await WriteAsync(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED_ADVISOR", ex.Message);
        }
        catch (NoQuotaAvailableException ex)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, ex.Code, ex.Message);
        }
        catch (ReferenceConflictException ex)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, ex.Code, ex.Message);
        }
        catch (PlanNotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, ex.Code, ex.Message);
        }
        catch (InvalidSaleDataException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, ex.Code, ex.Message);
        }
        catch (ConcurrencyConflictException ex)
        {
            // No se pudo resolver el conflicto tras los reintentos.
            _logger.LogWarning(ex, "Conflicto de concurrencia no resuelto.");
            await WriteAsync(context, StatusCodes.Status409Conflict, "CONCURRENCY_CONFLICT",
                "No se pudo completar la operación por alta concurrencia. Reintente.");
        }
        catch (Exception ex)
        {
            // Registro técnico sin datos personales.
            _logger.LogError(ex, "Error no controlado en {Path}.", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR",
                "Ocurrió un error inesperado.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string code, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            error = new { code, message }
        });

        await context.Response.WriteAsync(payload);
    }
}
