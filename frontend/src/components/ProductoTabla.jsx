import { formatoMoneda } from '../utils/formato';

export const STOCK_BAJO = 5;
const STOCK_REFERENCIA = 50; // con 50 unidades o más la barra se ve llena

function Medidor({ stock }) {
  const nivel = stock === 0 ? 'agotado' : stock <= STOCK_BAJO ? 'bajo' : 'ok';
  const porcentaje = Math.min(100, Math.round((stock / STOCK_REFERENCIA) * 100));

  return (
    <div className={`medidor medidor--${nivel}`}>
      <span className="medidor__valor">{stock}</span>
      <span className="medidor__pista" aria-hidden="true">
        <span className="medidor__barra" style={{ width: `${stock > 0 ? Math.max(porcentaje, 6) : 0}%` }} />
      </span>
      {nivel === 'agotado' && <span className="medidor__etiqueta">Agotado</span>}
      {nivel === 'bajo' && <span className="medidor__etiqueta">Stock bajo</span>}
    </div>
  );
}

function FilasEsqueleto() {
  return Array.from({ length: 5 }, (_, i) => (
    <tr key={i} className="fila-esqueleto" aria-hidden="true">
      <td><span className="esqueleto esqueleto--corto" /></td>
      <td><span className="esqueleto" /></td>
      <td><span className="esqueleto esqueleto--corto" /></td>
      <td><span className="esqueleto esqueleto--corto" /></td>
      <td />
    </tr>
  ));
}

export default function ProductoTabla({ items, cargando, busqueda, onNuevo, onEditar, onEliminar }) {
  const vacio = !cargando && items.length === 0;

  if (vacio) {
    return (
      <div className="vacio">
        {busqueda ? (
          <>
            <p className="vacio__titulo">Ningún producto coincide con «{busqueda}»</p>
            <p>Prueba con otro nombre o con el código completo.</p>
          </>
        ) : (
          <>
            <p className="vacio__titulo">Todavía no hay productos</p>
            <p>Registra el primero para empezar a controlar el inventario.</p>
            <button type="button" className="boton boton--primario" onClick={onNuevo}>
              Nuevo producto
            </button>
          </>
        )}
      </div>
    );
  }

  return (
    <div className="tabla-contenedor" aria-busy={cargando}>
      <table className="tabla">
        <caption className="solo-lectores">Listado de productos activos</caption>
        <thead>
          <tr>
            <th scope="col">Código</th>
            <th scope="col">Producto</th>
            <th scope="col" className="num">Precio</th>
            <th scope="col">Stock</th>
            <th scope="col"><span className="solo-lectores">Acciones</span></th>
          </tr>
        </thead>
        <tbody>
          {cargando && items.length === 0 ? (
            <FilasEsqueleto />
          ) : (
            items.map((p) => (
              <tr key={p.id}>
                <td data-label="Código"><code className="codigo">{p.codigo}</code></td>
                <td data-label="Producto">
                  <span className="producto__nombre">{p.nombre}</span>
                  {p.descripcion && <span className="producto__detalle">{p.descripcion}</span>}
                </td>
                <td data-label="Precio" className="num">{formatoMoneda(p.precio)}</td>
                <td data-label="Stock"><Medidor stock={p.stock} /></td>
                <td className="acciones">
                  <button type="button" className="boton boton--tenue" onClick={() => onEditar(p)} aria-label={`Editar ${p.nombre}`}>
                    Editar
                  </button>
                  <button type="button" className="boton boton--peligro" onClick={() => onEliminar(p)} aria-label={`Eliminar ${p.nombre}`}>
                    Eliminar
                  </button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
