# Gestión de productos (C# .NET 10 + React)

CRUD de productos con API en ASP.NET Core (.NET 10), SQL Server con procedimientos almacenados y frontend en React (Vite).

## Estructura

```
GestionProductos/
├─ backend/GestionProductos.Api/
│  ├─ Controllers/     recibe las peticiones y habla solo con el servicio
│  ├─ Services/        reglas de negocio; habla solo con el repositorio
│  ├─ Repositories/    única capa que habla con la BD (SqlCommand + procedimientos)
│  ├─ Interfaces/      IProductoService, IProductoRepository (un archivo cada una)
│  ├─ DTOs/            datos que entran y salen de la API (con validaciones)
│  ├─ Models/          entidad Producto (reflejo de la tabla)
│  ├─ Mappers/         Producto -> ProductoDto
│  ├─ Exceptions/      errores de negocio y de acceso a datos
│  └─ Middleware/      manejo central de errores y cabeceras de seguridad
├─ database/01_crear_base_de_datos.sql
└─ frontend/           React + Vite
```

## Puesta en marcha

1. **Base de datos**: ejecuta `database/01_crear_base_de_datos.sql` en SQL Server (crea `GestionProductosDb`, la tabla, los procedimientos y 8 productos de ejemplo).
2. **Cadena de conexión**: revisa `backend/GestionProductos.Api/appsettings.json`. Por defecto usa autenticación de Windows contra `localhost`. Con usuario y contraseña, no los pongas en el archivo:
   ```
   cd backend/GestionProductos.Api
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=GestionProductosDb;User Id=api_productos;Password=...;Encrypt=True;TrustServerCertificate=True"
   ```
3. **API** (necesita el SDK de .NET 10):
   ```
   dotnet dev-certs https --trust
   cd backend/GestionProductos.Api
   dotnet run --launch-profile https
   ```
   Queda en `https://localhost:7080`.
4. **Frontend** (necesita Node 20+):
   ```
   cd frontend
   npm install
   npm run dev
   ```
   Abre `http://localhost:5173`. Si cambias el puerto de la API, ajusta `frontend/.env.development`.

## Endpoints

| Método | Ruta                          | Descripción                              |
|--------|-------------------------------|------------------------------------------|
| GET    | `/api/productos?busqueda=&pagina=1&tamano=8` | Listado paginado con búsqueda |
| GET    | `/api/productos/{id}`         | Detalle                                  |
| POST   | `/api/productos`              | Crear                                    |
| PUT    | `/api/productos/{id}`         | Actualizar (el código no se modifica)    |
| DELETE | `/api/productos/{id}`         | Eliminar (borrado lógico)                |

## Seguridad aplicada (OWASP)

- **A03 Inyección**: solo procedimientos almacenados, parámetros tipados con `SqlDbType` y tamaño; nunca se concatena SQL. En el procedimiento de listado se escapan los comodines del `LIKE`.
- **Validación de entrada**: DataAnnotations en los DTOs (`[ApiController]` responde 400 solos), validación de negocio en el servicio y restricciones `CHECK`/`UNIQUE` en la BD. El cliente nunca recibe ni envía la entidad directa (evita mass assignment).
- **A05 Configuración**: cabeceras de seguridad, sin cabecera `Server`, HSTS fuera de desarrollo, CORS restringido a orígenes, métodos y cabeceras concretos, límite de tamaño de body.
- **A04 / A09 Diseño y registro**: errores como `ProblemDetails` sin stack traces ni mensajes de SQL; el detalle técnico solo va al log. Límite de 100 solicitudes por minuto por IP.
- **A02 / A07**: secretos fuera del repositorio con *user-secrets*; el script incluye (comentado) un usuario con permiso solo de `EXECUTE`.
- **XSS**: React escapa todo lo que pinta; no se usa `dangerouslySetInnerHTML`, y además se rechazan `<` y `>` en nombre y descripción.

