namespace GestionProductos.Api.DTOs;

public record ProductoDto(
    int Id,
    string Codigo,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);
