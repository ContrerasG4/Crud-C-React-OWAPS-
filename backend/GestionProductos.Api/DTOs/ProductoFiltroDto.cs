using System.ComponentModel.DataAnnotations;

namespace GestionProductos.Api.DTOs;

/// <summary>Parámetros de la query string del listado.</summary>
public class ProductoFiltroDto
{
    [StringLength(100, ErrorMessage = "La búsqueda admite máximo 100 caracteres.")]
    public string? Busqueda { get; set; }

    [Range(1, 100000, ErrorMessage = "La página debe ser mayor o igual a 1.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "El tamaño de página debe estar entre 1 y 50.")]
    public int Tamano { get; set; } = 10;
}
