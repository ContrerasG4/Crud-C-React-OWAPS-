import { useEffect, useState } from 'react';
import { productosApi } from './api/productosApi';
import { useDebounce } from './hooks/useDebounce';
import ProductoTabla, { STOCK_BAJO } from './components/ProductoTabla';
import ProductoFormulario from './components/ProductoFormulario';
import ModalConfirmar from './components/ModalConfirmar';
import Paginacion from './components/Paginacion';
import Toasts from './components/Toasts';

const TAMANO_PAGINA = 8;

export default function App() {
  const [datos, setDatos] = useState({ items: [], total: 0, pagina: 1, totalPaginas: 1 });
  const [pagina, setPagina] = useState(1);
  const [busqueda, setBusqueda] = useState('');
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState(null);
  const [recarga, setRecarga] = useState(0);

  const [formulario, setFormulario] = useState(null); // null = cerrado, { producto? } = abierto
  const [porEliminar, setPorEliminar] = useState(null);
  const [eliminando, setEliminando] = useState(false);
  const [avisos, setAvisos] = useState([]);

  const busquedaDiferida = useDebounce(busqueda.trim(), 350);

  useEffect(() => {
    setPagina(1);
  }, [busquedaDiferida]);

  useEffect(() => {
    const control = new AbortController();
    setCargando(true);
    setError(null);

    productosApi
      .listar({ busqueda: busquedaDiferida, pagina, tamano: TAMANO_PAGINA }, control.signal)
      .then(setDatos)
      .catch((e) => {
        if (e.name !== 'AbortError') setError(e.message);
      })
      .finally(() => {
        if (!control.signal.aborted) setCargando(false);
      });

    return () => control.abort();
  }, [busquedaDiferida, pagina, recarga]);

  const avisar = (texto, tipo = 'ok') => {
    const id = `${Date.now()}-${Math.random()}`;
    setAvisos((a) => [...a, { id, texto, tipo }]);
    setTimeout(() => setAvisos((a) => a.filter((x) => x.id !== id)), 4000);
  };

  const guardar = async (valores) => {
    if (formulario?.producto) {
      await productosApi.actualizar(formulario.producto.id, valores);
      avisar('Cambios guardados');
    } else {
      await productosApi.crear(valores);
      avisar('Producto creado');
      setPagina(1);
    }
    setFormulario(null);
    setRecarga((n) => n + 1);
  };

  const confirmarEliminar = async () => {
    setEliminando(true);
    try {
      await productosApi.eliminar(porEliminar.id);
      avisar(`«${porEliminar.nombre}» eliminado`);
      setPorEliminar(null);
      // Si era el único de la última página, retrocedemos una para no quedar en una página vacía.
      if (datos.items.length === 1 && pagina > 1) setPagina((p) => p - 1);
      else setRecarga((n) => n + 1);
    } catch (e) {
      avisar(e.message, 'error');
    } finally {
      setEliminando(false);
    }
  };

  const conStockBajo = datos.items.filter((p) => p.stock <= STOCK_BAJO).length;

  return (
    <main className="pagina">
      <header className="encabezado">
        <div>
          <h1>Inventario</h1>
          <p className="resumen">
            {datos.total} {datos.total === 1 ? 'producto activo' : 'productos activos'}
            {conStockBajo > 0 && `, ${conStockBajo} en esta página con stock bajo o agotado`}
          </p>
        </div>
        <button type="button" className="boton boton--primario" onClick={() => setFormulario({})}>
          Nuevo producto
        </button>
      </header>

      <div className="barra">
        <label htmlFor="buscar" className="solo-lectores">Buscar producto</label>
        <input
          id="buscar"
          type="search"
          className="entrada entrada--busqueda"
          placeholder="Buscar por nombre o código"
          value={busqueda}
          onChange={(e) => setBusqueda(e.target.value)}
          maxLength={100}
          autoComplete="off"
        />
      </div>

      {error ? (
        <div className="vacio vacio--error" role="alert">
          <p className="vacio__titulo">No se pudo cargar el inventario</p>
          <p>{error}</p>
          <button type="button" className="boton boton--tenue" onClick={() => setRecarga((n) => n + 1)}>
            Reintentar
          </button>
        </div>
      ) : (
        <>
          <ProductoTabla
            items={datos.items}
            cargando={cargando}
            busqueda={busquedaDiferida}
            onNuevo={() => setFormulario({})}
            onEditar={(producto) => setFormulario({ producto })}
            onEliminar={setPorEliminar}
          />
          <Paginacion
            pagina={pagina}
            totalPaginas={datos.totalPaginas}
            total={datos.total}
            tamano={TAMANO_PAGINA}
            deshabilitado={cargando}
            onCambiar={setPagina}
          />
        </>
      )}

      {formulario && (
        <ProductoFormulario
          producto={formulario.producto}
          onGuardar={guardar}
          onCerrar={() => setFormulario(null)}
        />
      )}

      {porEliminar && (
        <ModalConfirmar
          titulo="Eliminar producto"
          textoConfirmar="Eliminar"
          ocupado={eliminando}
          onConfirmar={confirmarEliminar}
          onCancelar={() => setPorEliminar(null)}
        >
          <p>
            Vas a eliminar <strong>{porEliminar.nombre}</strong> ({porEliminar.codigo}). Dejará de aparecer en el
            inventario.
          </p>
        </ModalConfirmar>
      )}

      <Toasts avisos={avisos} />
    </main>
  );
}
