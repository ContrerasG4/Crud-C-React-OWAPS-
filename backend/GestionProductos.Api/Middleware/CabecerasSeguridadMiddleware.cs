namespace GestionProductos.Api.Middleware;

/// <summary>Cabeceras defensivas para una API que solo devuelve JSON.</summary>
public class CabecerasSeguridadMiddleware
{
    private readonly RequestDelegate _siguiente;

    public CabecerasSeguridadMiddleware(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    public Task InvokeAsync(HttpContext contexto)
    {
        contexto.Response.OnStarting(() =>
        {
            var h = contexto.Response.Headers;
            h["X-Content-Type-Options"] = "nosniff";
            h["X-Frame-Options"] = "DENY";
            h["Referrer-Policy"] = "no-referrer";
            h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
            h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            h["Cache-Control"] = "no-store";
            return Task.CompletedTask;
        });

        return _siguiente(contexto);
    }
}
