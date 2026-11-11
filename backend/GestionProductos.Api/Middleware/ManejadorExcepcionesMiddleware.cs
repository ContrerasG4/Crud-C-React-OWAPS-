using GestionProductos.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GestionProductos.Api.Middleware;

/// Un solo punto donde las excepciones se convierten en respuestas.
/// El cliente nunca ve stack traces ni mensajes de SQL Server (A05 / A09 de OWASP).

public class ManejadorExcepcionesMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejadorExcepcionesMiddleware> _logger;

    public ManejadorExcepcionesMiddleware(RequestDelegate siguiente, ILogger<ManejadorExcepcionesMiddleware> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (OperationCanceledException) when (contexto.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión; no hay a quién responder.
        }
        catch (Exception ex)
        {
            if (contexto.Response.HasStarted) throw;

            var (estado, titulo, detalle) = ex switch
            {
                NoEncontradoException e => (StatusCodes.Status404NotFound, "Recurso no encontrado", e.Message),
                ConflictoException e => (StatusCodes.Status409Conflict, "Conflicto", e.Message),
                ReglaNegocioException e => (StatusCodes.Status400BadRequest, "Solicitud inválida", e.Message),
                BadHttpRequestException e => (e.StatusCode, "Solicitud inválida", "La solicitud no es válida."),
                _ => (StatusCodes.Status500InternalServerError, "Error interno",
                      "Ocurrió un error inesperado. Intenta de nuevo más tarde.")
            };

            if (estado >= 500)
                _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
            else
                _logger.LogWarning("{Titulo}: {Detalle}", titulo, detalle);

            var problema = new ProblemDetails
            {
                Status = estado,
                Title = titulo,
                Detail = detalle,
                Instance = contexto.Request.Path
            };
            problema.Extensions["traceId"] = contexto.TraceIdentifier;

            contexto.Response.StatusCode = estado;
            await contexto.Response.WriteAsJsonAsync(problema, options: null, contentType: "application/problem+json");
        }
    }
}
