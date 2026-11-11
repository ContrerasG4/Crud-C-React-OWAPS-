using System.ComponentModel.DataAnnotations;

namespace GestionProductos.Api.DTOs;

public class ProductoCrearDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [RegularExpression(@"^[A-Za-z0-9_-]{3,20}$",
        ErrorMessage = "El código debe tener de 3 a 20 caracteres: letras, números, guion o guion bajo.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "El nombre no puede contener los caracteres < o >.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción admite máximo 300 caracteres.")]
    [RegularExpression(@"^[^<>]*$", ErrorMessage = "La descripción no puede contener los caracteres < o >.")]
    public string? Descripcion { get; set; }

    [Range(0.01, 99999999.99, ErrorMessage = "El precio debe estar entre 0,01 y 99.999.999,99.")]
    public decimal Precio { get; set; }

    [Range(0, 1000000, ErrorMessage = "El stock debe estar entre 0 y 1.000.000.")]
    public int Stock { get; set; }
}
