import { useEffect, useRef } from 'react';

export default function ModalConfirmar({ titulo, children, textoConfirmar, ocupado, onConfirmar, onCancelar }) {
  const botonCancelar = useRef(null);

  useEffect(() => {
    botonCancelar.current?.focus();
  }, []);

  useEffect(() => {
    const alTeclear = (evento) => {
      if (evento.key === 'Escape' && !ocupado) onCancelar();
    };
    window.addEventListener('keydown', alTeclear);
    return () => window.removeEventListener('keydown', alTeclear);
  }, [ocupado, onCancelar]);

  return (
    <div className="velo" onMouseDown={(e) => e.target === e.currentTarget && !ocupado && onCancelar()}>
      <section className="modal modal--estrecho" role="alertdialog" aria-modal="true" aria-labelledby="titulo-confirmar">
        <header className="modal__cabecera">
          <h2 id="titulo-confirmar">{titulo}</h2>
        </header>
        <div className="modal__cuerpo">{children}</div>
        <footer className="modal__pie">
          <button type="button" ref={botonCancelar} className="boton boton--tenue" onClick={onCancelar} disabled={ocupado}>
            Cancelar
          </button>
          <button type="button" className="boton boton--peligro-solido" onClick={onConfirmar} disabled={ocupado}>
            {ocupado ? 'Eliminando…' : textoConfirmar}
          </button>
        </footer>
      </section>
    </div>
  );
}
