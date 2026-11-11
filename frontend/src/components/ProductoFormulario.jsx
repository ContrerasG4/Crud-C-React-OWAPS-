import { useEffect, useRef, useState } from 'react';
import { ApiError } from '../api/productosApi';

const VACIO = { codigo: '', nombre: '', descripcion: '', precio: '', stock: '' };
const SIN_ANGULARES = /[<>]/;

// Misma validación que el backend: el servidor siempre tiene la última palabra,
// pero así el usuario se entera antes de enviar.
function validar(v, editando) {
  const e = {};

  if (!editando && !/^[A-Za-z0-9_-]{3,20}$/.test(v.codigo.trim())) {
    e.codigo = 'Usa de 3 a 20 caracteres: letras, números, guion o guion bajo.';
  }

  const nombre = v.nombre.trim();
  if (nombre.length < 3 || nombre.length > 100) e.nombre = 'El nombre debe tener entre 3 y 100 caracteres.';
  else if (SIN_ANGULARES.test(nombre)) e.nombre = 'El nombre no puede contener los caracteres < o >.';

  if (v.descripcion.length > 300) e.descripcion = 'La descripción admite máximo 300 caracteres.';
  else if (SIN_ANGULARES.test(v.descripcion)) e.descripcion = 'La descripción no puede contener los caracteres < o >.';

  const precio = Number(v.precio);
  if (v.precio.trim() === '' || !Number.isFinite(precio) || precio < 0.01 || precio > 99999999.99) {
    e.precio = 'Ingresa un precio entre 0,01 y 99.999.999,99.';
  } else if (Math.round(precio * 100) / 100 !== precio) {
    e.precio = 'El precio admite máximo 2 decimales.';
  }

  const stock = Number(v.stock);
  if (v.stock.trim() === '' || !Number.isInteger(stock) || stock < 0 || stock > 1000000) {
    e.stock = 'Ingresa un número entero entre 0 y 1.000.000.';
  }

  return e;
}

function Campo({ id, etiqueta, error, ayuda, children }) {
  return (
    <div className={`campo${error ? ' campo--error' : ''}`}>
      <label htmlFor={id}>{etiqueta}</label>
      {children}
      {error ? <p className="campo__mensaje" id={`${id}-error`}>{error}</p> : ayuda && <p className="campo__ayuda">{ayuda}</p>}
    </div>
  );
}

export default function ProductoFormulario({ producto, onGuardar, onCerrar }) {
  const editando = Boolean(producto);
  const [valores, setValores] = useState(() =>
    producto
      ? {
          codigo: producto.codigo,
          nombre: producto.nombre,
          descripcion: producto.descripcion ?? '',
          precio: String(producto.precio),
          stock: String(producto.stock),
        }
      : VACIO,
  );
  const [errores, setErrores] = useState({});
  const [errorGeneral, setErrorGeneral] = useState(null);
  const [enviando, setEnviando] = useState(false);
  const primerCampo = useRef(null);

  useEffect(() => {
    primerCampo.current?.focus();
  }, []);

  useEffect(() => {
    const alTeclear = (evento) => {
      if (evento.key === 'Escape' && !enviando) onCerrar();
    };
    window.addEventListener('keydown', alTeclear);
    return () => window.removeEventListener('keydown', alTeclear);
  }, [enviando, onCerrar]);

  const cambiar = (campo) => (evento) => {
    const valor = evento.target.value;
    setValores((v) => ({ ...v, [campo]: valor }));
    setErrores((er) => (er[campo] ? { ...er, [campo]: undefined } : er));
  };

  const enviar = async (evento) => {
    evento.preventDefault();

    const encontrados = validar(valores, editando);
    setErrores(encontrados);
    if (Object.keys(encontrados).length > 0) return;

    const carga = {
      nombre: valores.nombre.trim(),
      descripcion: valores.descripcion.trim() || null,
      precio: Number(valores.precio),
      stock: Number(valores.stock),
    };
    if (!editando) carga.codigo = valores.codigo.trim();

    setEnviando(true);
    setErrorGeneral(null);
    try {
      await onGuardar(carga);
    } catch (error) {
      if (error instanceof ApiError && error.estado === 409) {
        setErrores({ codigo: error.message });
      } else if (error instanceof ApiError && error.errores) {
        setErrores(error.errores);
        setErrorGeneral('Revisa los campos marcados.');
      } else {
        setErrorGeneral(error.message);
      }
      setEnviando(false);
    }
  };

  return (
    <div className="velo" onMouseDown={(e) => e.target === e.currentTarget && !enviando && onCerrar()}>
      <section className="modal" role="dialog" aria-modal="true" aria-labelledby="titulo-formulario">
        <header className="modal__cabecera">
          <h2 id="titulo-formulario">{editando ? 'Editar producto' : 'Nuevo producto'}</h2>
        </header>

        <form onSubmit={enviar} noValidate>
          <div className="modal__cuerpo">
            <Campo
              id="codigo"
              etiqueta="Código"
              error={errores.codigo}
              ayuda={editando ? 'El código no se puede cambiar.' : 'Ejemplo: PRD-0009'}
            >
              <input
                id="codigo"
                ref={editando ? null : primerCampo}
                className="entrada entrada--mono"
                value={valores.codigo}
                onChange={cambiar('codigo')}
                disabled={editando}
                maxLength={20}
                autoComplete="off"
                aria-invalid={Boolean(errores.codigo)}
                aria-describedby={errores.codigo ? 'codigo-error' : undefined}
              />
            </Campo>

            <Campo id="nombre" etiqueta="Nombre" error={errores.nombre}>
              <input
                id="nombre"
                ref={editando ? primerCampo : null}
                className="entrada"
                value={valores.nombre}
                onChange={cambiar('nombre')}
                maxLength={100}
                autoComplete="off"
                aria-invalid={Boolean(errores.nombre)}
                aria-describedby={errores.nombre ? 'nombre-error' : undefined}
              />
            </Campo>

            <Campo
              id="descripcion"
              etiqueta="Descripción (opcional)"
              error={errores.descripcion}
              ayuda={`${valores.descripcion.length} de 300 caracteres`}
            >
              <textarea
                id="descripcion"
                className="entrada"
                rows={3}
                value={valores.descripcion}
                onChange={cambiar('descripcion')}
                maxLength={300}
                aria-invalid={Boolean(errores.descripcion)}
                aria-describedby={errores.descripcion ? 'descripcion-error' : undefined}
              />
            </Campo>

            <div className="campos-fila">
              <Campo id="precio" etiqueta="Precio (COP)" error={errores.precio}>
                <input
                  id="precio"
                  className="entrada"
                  type="number"
                  inputMode="decimal"
                  min="0.01"
                  step="0.01"
                  value={valores.precio}
                  onChange={cambiar('precio')}
                  aria-invalid={Boolean(errores.precio)}
                  aria-describedby={errores.precio ? 'precio-error' : undefined}
                />
              </Campo>

              <Campo id="stock" etiqueta="Unidades en stock" error={errores.stock}>
                <input
                  id="stock"
                  className="entrada"
                  type="number"
                  inputMode="numeric"
                  min="0"
                  step="1"
                  value={valores.stock}
                  onChange={cambiar('stock')}
                  aria-invalid={Boolean(errores.stock)}
                  aria-describedby={errores.stock ? 'stock-error' : undefined}
                />
              </Campo>
            </div>

            {errorGeneral && <p className="aviso-error" role="alert">{errorGeneral}</p>}
          </div>

          <footer className="modal__pie">
            <button type="button" className="boton boton--tenue" onClick={onCerrar} disabled={enviando}>
              Cancelar
            </button>
            <button type="submit" className="boton boton--primario" disabled={enviando}>
              {enviando ? 'Guardando…' : editando ? 'Guardar cambios' : 'Crear producto'}
            </button>
          </footer>
        </form>
      </section>
    </div>
  );
}
