import { useState } from 'react';

const FAIXAS = ['Branca', 'Cinza', 'Azul', 'Roxa', 'Marrom', 'Preta'];

// Componente de filtros da listagem — emite os valores ao pai via onFiltrar
export function AtletaFiltros({ onFiltrar }) {
  const [nome, setNome] = useState('');
  const [faixa, setFaixa] = useState('');

  function handleSubmit(e) {
    e.preventDefault();
    onFiltrar({ nome: nome || undefined, faixa: faixa || undefined });
  }

  function handleLimpar() {
    setNome('');
    setFaixa('');
    onFiltrar({});
  }

  return (
    <form className="filtros" onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Buscar por nome..."
        value={nome}
        onChange={(e) => setNome(e.target.value)}
      />
      <select value={faixa} onChange={(e) => setFaixa(e.target.value)}>
        <option value="">Todas as faixas</option>
        {FAIXAS.map((f) => (
          <option key={f} value={f}>{f}</option>
        ))}
      </select>
      <button type="submit" className="btn-primario">Filtrar</button>
      <button type="button" className="btn-secundario" onClick={handleLimpar}>
        Limpar
      </button>
    </form>
  );
}
