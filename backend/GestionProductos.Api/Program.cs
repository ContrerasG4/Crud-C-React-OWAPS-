using System.Threading.RateLimiting;
using GestionProductos.Api.Interfaces;
using GestionProductos.Api.Middleware;
using GestionProductos.Api.Repositories;
using GestionProductos.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opciones =>
{
    opciones.AddServerHeader = false;                 // no anunciar la tecnología del servidor
    opciones.Limits.MaxRequestBodySize = 32 * 1024;   // el body más grande legítimo pesa unos pocos KB
});

builder.Services.AddControllers();


builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

// CORS: solo los orígenes declarados en appsettings, con los métodos y cabeceras que realmente usamos.
var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(opciones => opciones.AddPolicy("Frontend", politica => politica
    .WithOrigins(origenes)
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .WithHeaders("Content-Type")));

// Límite por IP para frenar fuerza bruta y scraping.
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.UseMiddleware<ManejadorExcepcionesMiddleware>();
app.UseMiddleware<CabecerasSeguridadMiddleware>();

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseRateLimiter();

app.MapControllers();

app.Run();
