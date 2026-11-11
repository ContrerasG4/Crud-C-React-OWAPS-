import { useEffect, useState } from 'react';

export function useDebounce(valor, espera = 350) {
  const [retrasado, setRetrasado] = useState(valor);

  useEffect(() => {
    const id = setTimeout(() => setRetrasado(valor), espera);
    return () => clearTimeout(id);
  }, [valor, espera]);

  return retrasado;
}
