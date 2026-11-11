using GestionProductos.Api.DTOs;
using GestionProductos.Api.Models;

namespace GestionProductos.Api.Mappers;

public static class ProductoMapeo
{
    public static ProductoDto ADto(this Producto p) =>
        new(p.Id, p.Codigo, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.FechaCreacion, p.FechaActualizacion);
}
