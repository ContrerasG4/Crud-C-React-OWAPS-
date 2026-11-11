using GestionProductos.Api.DTOs;
using GestionProductos.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionProductos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _servicio;

    public ProductosController(IProductoService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginadoDto<ProductoDto>>> Listar(
        [FromQuery] ProductoFiltroDto filtro, CancellationToken ct) =>
        Ok(await _servicio.ListarAsync(filtro, ct));

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(int id, CancellationToken ct) =>
        Ok(await _servicio.ObtenerPorIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear([FromBody] ProductoCrearDto dto, CancellationToken ct)
    {
        var creado = await _servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int:min(1)}")]
    public async Task<ActionResult<ProductoDto>> Actualizar(
        int id, [FromBody] ProductoActualizarDto dto, CancellationToken ct) =>
        Ok(await _servicio.ActualizarAsync(id, dto, ct));

    [HttpDelete("{id:int:min(1)}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
