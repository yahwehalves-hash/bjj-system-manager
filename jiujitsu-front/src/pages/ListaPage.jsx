import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { atletasApi } from '../api/atletasApi';
import { AtletaFiltros } from '../components/AtletaFiltros';
import { ConfirmDialog } from '../components/ConfirmDialog';

const GRAUS = ['', '1°', '2°', '3°', '4°'];

const COR_FAIXA = {
  Branca:  '#ffffff',
  Cinza:   '#9e9e9e',
  Azul:    '#1565c0',
  Roxa:    '#6a1b9a',
  Marrom:  '#5d4037',
  Preta:   '#212121',
};

export function ListaPage() {
  const navigate = useNavigate();

  const [dados, setDados] = useState({ itens: [], totalItens: 0, pagina: 1, tamanhoPagina: 10 });
  const [filtros, setFiltros] = useState({});
  const [pagina, setPagina] = useState(1);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState('');
  const [aviso, setAviso] = useState('');
  const [confirmarExclusao, setConfirmarExclusao] = useState(null); // id do atleta a excluir

  useEffect(() => {
    buscar();
  }, [filtros, pagina]);

  async function buscar() {
    setCarregando(true);
    setErro('');
    try {
      const resultado = await atletasApi.listar({
        ...filtros,
        pagina,
        tamanhoPagina: 10,
      });
      setDados(resultado);
    } catch {
      setErro('Erro ao carregar atletas. Verifique se a API está em execução.');
    } finally {
      setCarregando(false);
    }
  }

  function handleFiltrar(novosFiltros) {
    setFiltros(novosFiltros);
    setPagina(1);
  }

  async function handleExcluir(id) {
    try {
      await atletasApi.excluir(id);
      setAviso('Exclusão enviada para processamento. A lista será atualizada em instantes.');
      setConfirmarExclusao(null);
      // Aguarda o Worker processar e recarrega
      setTimeout(() => buscar(), 1500);
    } catch {
      setErro('Erro ao excluir atleta.');
      setConfirmarExclusao(null);
    }
  }

  const totalPaginas = Math.ceil(dados.totalItens / dados.tamanhoPagina);

  return (
    <div className="pagina">
      <div className="cabecalho">
        <h1>Atletas</h1>
        <button className="btn-primario" onClick={() => navigate('/atletas/novo')}>
          + Novo Atleta
        </button>
      </div>

      {aviso && <div className="alerta-info">{aviso}</div>}
      {erro   && <div className="alerta-erro">{erro}</div>}

      <AtletaFiltros onFiltrar={handleFiltrar} />

      {carregando ? (
        <p className="carregando">Carregando...</p>
      ) : dados.itens.length === 0 ? (
        <p className="vazio">Nenhum atleta encontrado.</p>
      ) : (
        <>
          <table className="tabela">
            <thead>
              <tr>
                <th>Nome</th>
                <th>CPF</th>
                <th>Faixa</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {dados.itens.map((atleta) => (
                <tr key={atleta.id}>
                  <td>{atleta.nomeCompleto}</td>
                  <td>{atleta.cpf}</td>
                  <td>
                    <div className="faixa-cell">
                      <span
                        className="faixa-barra"
                        style={{
                          background: COR_FAIXA[atleta.faixa] ?? '#ccc',
                          border: atleta.faixa === 'Branca' ? '1px solid #ccc' : 'none',
                        }}
                      />
                      <span>
                        {atleta.faixa}
                        {atleta.grau > 0 && ` — ${GRAUS[atleta.grau]} grau`}
                      </span>
                    </div>
                  </td>
                  <td className="acoes">
                    <button
                      className="btn-secundario btn-sm"
                      onClick={() => navigate(`/atletas/${atleta.id}/editar`)}
                    >
                      Editar
                    </button>
                    <button
                      className="btn-perigo btn-sm"
                      onClick={() => setConfirmarExclusao(atleta.id)}
                    >
                      Excluir
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Paginação */}
          {totalPaginas > 1 && (
            <div className="paginacao">
              <button
                disabled={pagina === 1}
                onClick={() => setPagina((p) => p - 1)}
              >
                ← Anterior
              </button>
              <span>
                Página {pagina} de {totalPaginas} ({dados.totalItens} atletas)
              </span>
              <button
                disabled={pagina === totalPaginas}
                onClick={() => setPagina((p) => p + 1)}
              >
                Próxima →
              </button>
            </div>
          )}
        </>
      )}

      {/* Dialog de confirmação de exclusão */}
      {confirmarExclusao && (
        <ConfirmDialog
          mensagem="Deseja realmente excluir este atleta? A ação não pode ser desfeita."
          onConfirmar={() => handleExcluir(confirmarExclusao)}
          onCancelar={() => setConfirmarExclusao(null)}
        />
      )}
    </div>
  );
}
