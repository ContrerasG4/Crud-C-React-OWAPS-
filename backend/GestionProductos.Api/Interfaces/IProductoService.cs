using GestionProductos.Api.DTOs;

namespace GestionProductos.Api.Interfaces;

public interface IProductoService
{
    Task<ResultadoPaginadoDto<ProductoDto>> ListarAsync(ProductoFiltroDto filtro, CancellationToken ct);
    Task<ProductoDto> ObtenerPorIdAsync(int id, CancellationToken ct);
    Task<ProductoDto> CrearAsync(ProductoCrearDto dto, CancellationToken ct);
    Task<ProductoDto> ActualizarAsync(int id, ProductoActualizarDto dto, CancellationToken ct);
    Task EliminarAsync(int id, CancellationToken ct);
}
