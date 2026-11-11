using GestionProductos.Api.Models;

namespace GestionProductos.Api.Interfaces;

public interface IProductoRepository
{
    Task<(IReadOnlyList<Producto> Items, int Total)> ListarAsync(string? busqueda, int pagina, int tamanoPagina, CancellationToken ct);
    Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct);
    Task<int> CrearAsync(Producto producto, CancellationToken ct);
    Task<bool> ActualizarAsync(Producto producto, CancellationToken ct);
    Task<bool> EliminarAsync(int id, CancellationToken ct);
}
