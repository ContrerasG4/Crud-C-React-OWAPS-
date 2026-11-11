export default function Paginacion({ pagina, totalPaginas, total, tamano, deshabilitado, onCambiar }) {
  if (total === 0) return null;

  const desde = (pagina - 1) * tamano + 1;
  const hasta = Math.min(pagina * tamano, total);

  return (
    <nav className="paginacion" aria-label="Paginación">
      <p>
        Mostrando {desde}–{hasta} de {total}
      </p>
      <div className="paginacion__botones">
        <button
          type="button"
          className="boton boton--tenue"
          disabled={deshabilitado || pagina <= 1}
          onClick={() => onCambiar(pagina - 1)}
        >
          Anterior
        </button>
        <span className="paginacion__estado">
          Página {pagina} de {totalPaginas}
        </span>
        <button
          type="button"
          className="boton boton--tenue"
          disabled={deshabilitado || pagina >= totalPaginas}
          onClick={() => onCambiar(pagina + 1)}
        >
          Siguiente
        </button>
      </div>
    </nav>
  );
}
