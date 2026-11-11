/* ============================================================
   GestionProductosDb - tabla, procedimientos almacenados y datos de prueba
   Ejecutar completo en SSMS / Azure Data Studio (requiere SQL Server 2016 SP1+).
   ============================================================ */

IF DB_ID(N'GestionProductosDb') IS NULL
    CREATE DATABASE GestionProductosDb;
GO

USE GestionProductosDb;
GO
 
IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Producto
    (
        Id                  INT IDENTITY(1,1)  NOT NULL,
        Codigo              VARCHAR(20)        NOT NULL,
        Nombre              NVARCHAR(100)      NOT NULL,
        Descripcion         NVARCHAR(300)      NULL,
        Precio              DECIMAL(10,2)      NOT NULL,
        Stock               INT                NOT NULL,
        Activo              BIT                NOT NULL CONSTRAINT DF_Producto_Activo DEFAULT (1),
        FechaCreacion       DATETIME2(0)       NOT NULL CONSTRAINT DF_Producto_FechaCreacion DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion  DATETIME2(0)       NULL,

        CONSTRAINT PK_Producto        PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Producto_Precio CHECK (Precio > 0),
        CONSTRAINT CK_Producto_Stock  CHECK (Stock >= 0)
    );

    -- El código es único aunque el producto esté dado de baja (borrado lógico).
    CREATE UNIQUE NONCLUSTERED INDEX UX_Producto_Codigo ON dbo.Producto (Codigo);
END
GO

/* ---------- Listado paginado con búsqueda ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Listar
    @Busqueda     NVARCHAR(100) = NULL,
    @Pagina       INT = 1,
    @TamanoPagina INT = 10,
    @Total        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina NOT BETWEEN 1 AND 50 SET @TamanoPagina = 10;

    -- Se escapan los comodines para que "50%" se busque literal y no como patrón.
    DECLARE @Patron NVARCHAR(250) = NULL;
    IF @Busqueda IS NOT NULL AND LEN(LTRIM(RTRIM(@Busqueda))) > 0
        SET @Patron = N'%' + REPLACE(REPLACE(REPLACE(@Busqueda, N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';

    SELECT @Total = COUNT(*)
    FROM dbo.Producto
    WHERE Activo = 1
      AND (@Patron IS NULL OR Nombre LIKE @Patron OR Codigo LIKE @Patron);

    SELECT Id, Codigo, Nombre, Descripcion, Precio, Stock, Activo, FechaCreacion, FechaActualizacion
    FROM dbo.Producto
    WHERE Activo = 1
      AND (@Patron IS NULL OR Nombre LIKE @Patron OR Codigo LIKE @Patron)
    ORDER BY Id DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END
GO

/* ---------- Detalle ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Producto_ObtenerPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Codigo, Nombre, Descripcion, Precio, Stock, Activo, FechaCreacion, FechaActualizacion
    FROM dbo.Producto
    WHERE Id = @Id AND Activo = 1;
END
GO

/* ---------- Crear: devuelve el Id generado ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Crear
    @Codigo      VARCHAR(20),
    @Nombre      NVARCHAR(100),
    @Descripcion NVARCHAR(300) = NULL,
    @Precio      DECIMAL(10,2),
    @Stock       INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = @Codigo)
        THROW 51000, N'Ya existe un producto con ese código.', 1;

    INSERT INTO dbo.Producto (Codigo, Nombre, Descripcion, Precio, Stock)
    VALUES (@Codigo, @Nombre, @Descripcion, @Precio, @Stock);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
END
GO

/* ---------- Actualizar: devuelve las filas afectadas (0 = no existe) ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Actualizar
    @Id          INT,
    @Nombre      NVARCHAR(100),
    @Descripcion NVARCHAR(300) = NULL,
    @Precio      DECIMAL(10,2),
    @Stock       INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Producto
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        Precio = @Precio,
        Stock = @Stock,
        FechaActualizacion = SYSUTCDATETIME()
    WHERE Id = @Id AND Activo = 1;

    SELECT @@ROWCOUNT AS Filas;
END
GO

/* ---------- Eliminar (borrado lógico) ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Producto
    SET Activo = 0,
        FechaActualizacion = SYSUTCDATETIME()
    WHERE Id = @Id AND Activo = 1;

    SELECT @@ROWCOUNT AS Filas;
END
GO

/* ---------- Datos de prueba ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Producto)
BEGIN
    INSERT INTO dbo.Producto (Codigo, Nombre, Descripcion, Precio, Stock) VALUES
    ('PRD-0001', N'Portátil Lenovo IdeaPad 3 15"',  N'Ryzen 5, 16 GB de RAM, SSD de 512 GB.',        2399000, 12),
    ('PRD-0002', N'Mouse inalámbrico Logitech M170', N'Receptor USB, batería de hasta 12 meses.',       39900, 64),
    ('PRD-0003', N'Teclado mecánico Redragon Kumara',N'Switches rojos, distribución en español.',      159900,  4),
    ('PRD-0004', N'Monitor LG 24" Full HD',          N'Panel IPS, 75 Hz, entradas HDMI y VGA.',         549000,  9),
    ('PRD-0005', N'Silla ergonómica de oficina',     N'Respaldo en malla, soporte lumbar ajustable.',   689000,  0),
    ('PRD-0006', N'Cable HDMI 2.0 de 2 m',           NULL,                                               19900, 150),
    ('PRD-0007', N'Audífonos Sony WH-CH520',         N'Bluetooth, 50 horas de batería.',                229000, 18),
    ('PRD-0008', N'Disco SSD Kingston 1 TB',         N'NVMe M.2, lectura de hasta 3.500 MB/s.',         319000,  3);
END
GO

/* ------------------------------------------------------------------
   Recomendado fuera de desarrollo: usuario de la API con el mínimo privilegio.
   La aplicación solo necesita ejecutar los procedimientos, no leer/escribir tablas.
   (Descomenta, cambia la contraseña y ajusta la cadena de conexión.)

   CREATE LOGIN api_productos WITH PASSWORD = N'<<contraseña-larga-y-única>>';
   CREATE USER  api_productos FOR LOGIN api_productos;
   GRANT EXECUTE ON SCHEMA::dbo TO api_productos;
   ------------------------------------------------------------------ */
      CREATE LOGIN api_productos WITH PASSWORD = N'<<MiP@ssw0rdSegura2026!?>>';
      CREATE USER  api_productos FOR LOGIN api_productos;
      GRANT EXECUTE ON SCHEMA::dbo TO api_productos;