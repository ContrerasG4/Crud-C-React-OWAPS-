using GestionProductos.Api.DTOs;
using GestionProductos.Api.Exceptions;
using GestionProductos.Api.Interfaces;
using GestionProductos.Api.Mappers;
using GestionProductos.Api.Models;

namespace GestionProductos.Api.Services;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repositorio;

    public ProductoService(IProductoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ResultadoPaginadoDto<ProductoDto>> ListarAsync(ProductoFiltroDto filtro, CancellationToken ct)
    {
        var busqueda = string.IsNullOrWhiteSpace(filtro.Busqueda) ? null : filtro.Busqueda.Trim();

        var (items, total) = await _repositorio.ListarAsync(busqueda, filtro.Pagina, filtro.Tamano, ct);

        return new ResultadoPaginadoDto<ProductoDto>(
            items.Select(p => p.ADto()).ToList(), total, filtro.Pagina, filtro.Tamano);
    }

    public async Task<ProductoDto> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        var producto = await _repositorio.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException($"No existe un producto con id {id}.");

        return producto.ADto();
    }

    public async Task<ProductoDto> CrearAsync(ProductoCrearDto dto, CancellationToken ct)
    {
        ValidarPrecio(dto.Precio);

        var nuevo = new Producto
        {
            Codigo = dto.Codigo.Trim().ToUpperInvariant(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = LimpiarOpcional(dto.Descripcion),
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        var id = await _repositorio.CrearAsync(nuevo, ct);
        return await ObtenerPorIdAsync(id, ct);
    }

    public async Task<ProductoDto> ActualizarAsync(int id, ProductoActualizarDto dto, CancellationToken ct)
    {
        ValidarPrecio(dto.Precio);

        var producto = new Producto
        {
            Id = id,
            Nombre = dto.Nombre.Trim(),
            Descripcion = LimpiarOpcional(dto.Descripcion),
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        if (!await _repositorio.ActualizarAsync(producto, ct))
            throw new NoEncontradoException($"No existe un producto con id {id}.");

        return await ObtenerPorIdAsync(id, ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        if (!await _repositorio.EliminarAsync(id, ct))
            throw new NoEncontradoException($"No existe un producto con id {id}.");
    }

    private static void ValidarPrecio(decimal precio)
    {
        // La columna es DECIMAL(10,2): preferimos rechazar a que SQL Server redondee en silencio.
        if (decimal.Round(precio, 2) != precio)
            throw new ReglaNegocioException("El precio admite máximo 2 decimales.");
    }

    private static string? LimpiarOpcional(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
