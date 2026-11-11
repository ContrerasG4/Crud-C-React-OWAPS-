export default function Toasts({ avisos }) {
  return (
    <div className="avisos" aria-live="polite">
      {avisos.map((a) => (
        <p key={a.id} className={`aviso aviso--${a.tipo}`}>
          {a.texto}
        </p>
      ))}
    </div>
  );
}
