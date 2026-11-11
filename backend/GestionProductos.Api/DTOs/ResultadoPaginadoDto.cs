namespace GestionProductos.Api.DTOs;

public record ResultadoPaginadoDto<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Pagina,
    int TamanoPagina)
{
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Total / (double)TamanoPagina));
}
