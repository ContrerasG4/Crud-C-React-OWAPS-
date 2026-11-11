using System.Data;
using GestionProductos.Api.Exceptions;
using GestionProductos.Api.Interfaces;
using GestionProductos.Api.Models;
using Microsoft.Data.SqlClient;

namespace GestionProductos.Api.Repositories;

public class ProductoRepository : IProductoRepository
{
    // Código que lanza usp_Producto_Crear con THROW cuando el código ya existe.
    // 2601 y 2627 cubren la carrera entre dos inserts simultáneos (índice único).
    private const int ErrorCodigoDuplicado = 51000;
    private const int ErrorIndiceUnico = 2601;
    private const int ErrorLlaveUnica = 2627;

    private readonly string _cadenaConexion;
    private readonly ILogger<ProductoRepository> _logger;

    public ProductoRepository(IConfiguration configuracion, ILogger<ProductoRepository> logger)
    {
        _cadenaConexion = configuracion.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");
        _logger = logger;
    }

    public async Task<(IReadOnlyList<Producto> Items, int Total)> ListarAsync(
        string? busqueda, int pagina, int tamanoPagina, CancellationToken ct)
    {
        try
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = NuevoComando(conexion, "dbo.usp_Producto_Listar");

            comando.Parameters.Add("@Busqueda", SqlDbType.NVarChar, 100).Value = (object?)busqueda ?? DBNull.Value;
            comando.Parameters.Add("@Pagina", SqlDbType.Int).Value = pagina;
            comando.Parameters.Add("@TamanoPagina", SqlDbType.Int).Value = tamanoPagina;

            var total = new SqlParameter("@Total", SqlDbType.Int) { Direction = ParameterDirection.Output };
            comando.Parameters.Add(total);

            await conexion.OpenAsync(ct);

            var items = new List<Producto>();
            using (var lector = await comando.ExecuteReaderAsync(ct))
            {
                while (await lector.ReadAsync(ct))
                    items.Add(Mapear(lector));
            }

            // Los parámetros OUTPUT solo se llenan cuando el lector ya se cerró.
            return (items, Convert.ToInt32(total.Value));
        }
        catch (SqlException ex)
        {
            throw Fallo(ex, "listar los productos");
        }
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        try
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = NuevoComando(conexion, "dbo.usp_Producto_ObtenerPorId");
            comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await conexion.OpenAsync(ct);
            using var lector = await comando.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);

            return await lector.ReadAsync(ct) ? Mapear(lector) : null;
        }
        catch (SqlException ex)
        {
            throw Fallo(ex, "consultar el producto");
        }
    }

    public async Task<int> CrearAsync(Producto producto, CancellationToken ct)
    {
        try
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = NuevoComando(conexion, "dbo.usp_Producto_Crear");

            comando.Parameters.Add("@Codigo", SqlDbType.VarChar, 20).Value = producto.Codigo;
            comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = producto.Nombre;
            comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 300).Value = (object?)producto.Descripcion ?? DBNull.Value;
            AgregarDecimal(comando, "@Precio", producto.Precio);
            comando.Parameters.Add("@Stock", SqlDbType.Int).Value = producto.Stock;

            await conexion.OpenAsync(ct);
            var nuevoId = await comando.ExecuteScalarAsync(ct);

            return Convert.ToInt32(nuevoId);
        }
        catch (SqlException ex) when (ex.Number is ErrorCodigoDuplicado or ErrorIndiceUnico or ErrorLlaveUnica)
        {
            throw new ConflictoException("Ya existe un producto con ese código.");
        }
        catch (SqlException ex)
        {
            throw Fallo(ex, "crear el producto");
        }
    }

    public async Task<bool> ActualizarAsync(Producto producto, CancellationToken ct)
    {
        try
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = NuevoComando(conexion, "dbo.usp_Producto_Actualizar");

            comando.Parameters.Add("@Id", SqlDbType.Int).Value = producto.Id;
            comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = producto.Nombre;
            comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 300).Value = (object?)producto.Descripcion ?? DBNull.Value;
            AgregarDecimal(comando, "@Precio", producto.Precio);
            comando.Parameters.Add("@Stock", SqlDbType.Int).Value = producto.Stock;

            await conexion.OpenAsync(ct);
            return Convert.ToInt32(await comando.ExecuteScalarAsync(ct)) > 0;
        }
        catch (SqlException ex)
        {
            throw Fallo(ex, "actualizar el producto");
        }
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct)
    {
        try
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = NuevoComando(conexion, "dbo.usp_Producto_Eliminar");
            comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await conexion.OpenAsync(ct);
            return Convert.ToInt32(await comando.ExecuteScalarAsync(ct)) > 0;
        }
        catch (SqlException ex)
        {
            throw Fallo(ex, "eliminar el producto");
        }
    }

    // ---- helpers ----

    private static SqlCommand NuevoComando(SqlConnection conexion, string procedimiento) =>
        new(procedimiento, conexion)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 15
        };

    private static void AgregarDecimal(SqlCommand comando, string nombre, decimal valor)
    {
        var parametro = comando.Parameters.Add(nombre, SqlDbType.Decimal);
        parametro.Precision = 10;
        parametro.Scale = 2;
        parametro.Value = valor;
    }

    private static Producto Mapear(SqlDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("Id")),
        Codigo = r.GetString(r.GetOrdinal("Codigo")),
        Nombre = r.GetString(r.GetOrdinal("Nombre")),
        Descripcion = r.IsDBNull(r.GetOrdinal("Descripcion")) ? null : r.GetString(r.GetOrdinal("Descripcion")),
        Precio = r.GetDecimal(r.GetOrdinal("Precio")),
        Stock = r.GetInt32(r.GetOrdinal("Stock")),
        Activo = r.GetBoolean(r.GetOrdinal("Activo")),
        FechaCreacion = DateTime.SpecifyKind(r.GetDateTime(r.GetOrdinal("FechaCreacion")), DateTimeKind.Utc),
        FechaActualizacion = r.IsDBNull(r.GetOrdinal("FechaActualizacion"))
            ? null
            : DateTime.SpecifyKind(r.GetDateTime(r.GetOrdinal("FechaActualizacion")), DateTimeKind.Utc)
    };

    private AccesoDatosException Fallo(SqlException ex, string operacion)
    {
        _logger.LogError(ex, "Error SQL al {Operacion} (número {Numero})", operacion, ex.Number);
        return new AccesoDatosException($"No se pudo {operacion}.", ex);
    }
}
