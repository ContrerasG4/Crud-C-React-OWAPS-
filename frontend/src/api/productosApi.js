const BASE = (import.meta.env.VITE_API_URL ?? 'https://localhost:7080/api').replace(/\/$/, '');

export class ApiError extends Error {
  constructor(estado, mensaje, errores = null) {
    super(mensaje);
    this.estado = estado;
    this.errores = errores; // { campo: 'mensaje' } cuando el backend rechaza la validación
  }
}

// ASP.NET devuelve las claves como "Nombre"; en el formulario usamos "nombre".
function normalizarErrores(errores) {
  if (!errores || typeof errores !== 'object') return null;
  const resultado = {};
  for (const [campo, mensajes] of Object.entries(errores)) {
    const clave = campo.charAt(0).toLowerCase() + campo.slice(1);
    resultado[clave] = Array.isArray(mensajes) ? mensajes[0] : String(mensajes);
  }
  return Object.keys(resultado).length ? resultado : null;
}

function mensajeDe(datos, estado) {
  if (estado === 429) return 'Hiciste demasiadas solicitudes seguidas. Espera un momento e intenta de nuevo.';
  if (estado >= 500) return 'El servidor tuvo un problema. Intenta de nuevo en unos minutos.';
  return datos?.detail ?? datos?.title ?? 'No se pudo completar la solicitud.';
}

async function solicitar(ruta, opciones = {}) {
  let respuesta;
  try {
    respuesta = await fetch(`${BASE}${ruta}`, {
      ...opciones,
      headers: { 'Content-Type': 'application/json', ...opciones.headers },
    });
  } catch (error) {
    if (error.name === 'AbortError') throw error;
    throw new ApiError(0, 'No hay conexión con el servidor. Verifica que la API esté en ejecución.');
  }

  if (respuesta.status === 204) return null;

  const datos = await respuesta.json().catch(() => null);
  if (!respuesta.ok) {
    throw new ApiError(respuesta.status, mensajeDe(datos, respuesta.status), normalizarErrores(datos?.errors));
  }
  return datos;
}

export const productosApi = {
  listar({ busqueda = '', pagina = 1, tamano = 8 } = {}, signal) {
    const params = new URLSearchParams({ pagina, tamano });
    if (busqueda) params.set('busqueda', busqueda);
    return solicitar(`/productos?${params}`, { signal });
  },
  crear(datos) {
    return solicitar('/productos', { method: 'POST', body: JSON.stringify(datos) });
  },
  actualizar(id, datos) {
    return solicitar(`/productos/${id}`, { method: 'PUT', body: JSON.stringify(datos) });
  },
  eliminar(id) {
    return solicitar(`/productos/${id}`, { method: 'DELETE' });
  },
};
