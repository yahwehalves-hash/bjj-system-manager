import { useEffect, useState } from 'react';
import { atletasApi } from '../api/atletasApi';
import { planosApi, matriculasApi } from '../api/planosApi';

const PERIODICIDADES = ['Mensal', 'Trimestral', 'Semestral', 'Anual'];

const PERIODICIDADE_LABEL = {
  Mensal: 'Mensal',
  Trimestral: 'Trimestral (3 meses)',
  Semestral: 'Semestral (6 meses)',
  Anual: 'Anual (12 meses)',
};

function hoje() {
  return new Date().toISOString().slice(0, 10);
}

const PLANO_VAZIO = { nome: '', descricao: '', valor: '', periodicidade: 'Mensal', filialId: null };

export default function PlanosPage() {
  const [tab, setTab] = useState('planos');

  // Planos
  const [planos, setPlanos] = useState([]);
  const [formPlano, setFormPlano] = useState(PLANO_VAZIO);
  const [editando, setEditando] = useState(null);
  const [showForm, setShowForm] = useState(false);

  // Matrículas
  const [atletas, setAtletas] = useState([]);
  const [atletaSel, setAtletaSel] = useState('');
  const [matriculas, setMatriculas] = useState([]);
  const [formMatricula, setFormMatricula] = useState({
    atletaId: '', planoId: '', dataInicio: hoje(), dataFim: '', valorCustomizado: '', observacao: '',
  });
  const [showFormMatricula, setShowFormMatricula] = useState(false);

  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState('');
  const [aviso, setAviso] = useState('');

  useEffect(() => {
    carregarPlanos();
  }, []);

  useEffect(() => {
    if (tab === 'matriculas') {
      carregarAtletas();
    }
  }, [tab]);

  useEffect(() => {
    if (atletaSel) carregarMatriculas(atletaSel);
  }, [atletaSel]);

  async function carregarPlanos() {
    setCarregando(true);
    try {
      setPlanos(await planosApi.listar());
    } catch {
      setErro('Erro ao carregar planos.');
    } finally {
      setCarregando(false);
    }
  }

  async function carregarAtletas() {
    try {
      const res = await atletasApi.listar({ pagina: 1, tamanhoPagina: 200 });
      setAtletas(res.itens);
    } catch {
      setErro('Erro ao carregar atletas.');
    }
  }

  async function carregarMatriculas(atletaId) {
    try {
      setMatriculas(await matriculasApi.listar(atletaId));
    } catch {
      setErro('Erro ao carregar matrículas.');
    }
  }

  function abrirNovoPlano() {
    setFormPlano(PLANO_VAZIO);
    setEditando(null);
    setShowForm(true);
  }

  function abrirEditarPlano(plano) {
    setFormPlano({
      nome: plano.nome,
      descricao: plano.descricao ?? '',
      valor: String(plano.valor),
      periodicidade: plano.periodicidade,
      filialId: plano.filialId,
    });
    setEditando(plano.id);
    setShowForm(true);
  }

  async function salvarPlano(e) {
    e.preventDefault();
    setErro('');
    const dados = {
      ...formPlano,
      valor: parseFloat(formPlano.valor),
      filialId: formPlano.filialId || null,
    };
    try {
      if (editando) {
        await planosApi.atualizar(editando, dados);
      } else {
        await planosApi.criar(dados);
      }
      setAviso('Plano salvo com sucesso.');
      setShowForm(false);
      carregarPlanos();
      setTimeout(() => setAviso(''), 3000);
    } catch {
      setErro('Erro ao salvar plano.');
    }
  }

  async function desativarPlano(id) {
    if (!confirm('Desativar este plano?')) return;
    try {
      await planosApi.desativar(id);
      setAviso('Plano desativado.');
      carregarPlanos();
      setTimeout(() => setAviso(''), 3000);
    } catch {
      setErro('Erro ao desativar plano.');
    }
  }

  async function criarMatricula(e) {
    e.preventDefault();
    setErro('');
    const dados = {
      atletaId: formMatricula.atletaId,
      planoId: formMatricula.planoId,
      dataInicio: formMatricula.dataInicio,
      dataFim: formMatricula.dataFim || null,
      valorCustomizado: formMatricula.valorCustomizado ? parseFloat(formMatricula.valorCustomizado) : null,
      observacao: formMatricula.observacao || null,
    };
    try {
      await matriculasApi.criar(dados);
      setAviso('Matrícula criada com sucesso.');
      setShowFormMatricula(false);
      if (formMatricula.atletaId) carregarMatriculas(formMatricula.atletaId);
      if (atletaSel) carregarMatriculas(atletaSel);
      setTimeout(() => setAviso(''), 3000);
    } catch {
      setErro('Erro ao criar matrícula.');
    }
  }

  async function cancelarMatricula(id) {
    if (!confirm('Cancelar esta matrícula?')) return;
    try {
      await matriculasApi.cancelar(id, 'Cancelado pelo gestor.');
      setAviso('Matrícula cancelada.');
      if (atletaSel) carregarMatriculas(atletaSel);
      setTimeout(() => setAviso(''), 3000);
    } catch {
      setErro('Erro ao cancelar matrícula.');
    }
  }

  function formatarData(d) {
    if (!d) return '—';
    const [a, m, dia] = String(d).split('-');
    return `${dia}/${m}/${a}`;
  }

  return (
    <div className="pagina">
      <div className="cabecalho">
        <h1>Planos e Matrículas</h1>
      </div>

      {aviso && <div className="alerta-info">{aviso}</div>}
      {erro && <div className="alerta-erro">{erro}</div>}

      <div style={{ display: 'flex', gap: 8, marginBottom: 20 }}>
        {[{ id: 'planos', label: 'Planos' }, { id: 'matriculas', label: 'Matrículas' }].map((t) => (
          <button
            key={t.id}
            className={tab === t.id ? 'btn-primario btn-sm' : 'btn-secundario btn-sm'}
            onClick={() => setTab(t.id)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* ── TAB PLANOS ─────────────────────────────────── */}
      {tab === 'planos' && (
        <>
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 16 }}>
            <button className="btn-primario btn-sm" onClick={abrirNovoPlano}>+ Novo Plano</button>
          </div>

          {carregando ? (
            <p className="carregando">Carregando...</p>
          ) : planos.length === 0 ? (
            <p className="vazio">Nenhum plano cadastrado.</p>
          ) : (
            <table className="tabela">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Periodicidade</th>
                  <th>Valor</th>
                  <th>Descrição</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {planos.map((p) => (
                  <tr key={p.id}>
                    <td>{p.nome}</td>
                    <td>{PERIODICIDADE_LABEL[p.periodicidade] ?? p.periodicidade}</td>
                    <td>R$ {Number(p.valor).toFixed(2).replace('.', ',')}</td>
                    <td>{p.descricao || '—'}</td>
                    <td className="acoes">
                      <button className="btn-secundario btn-sm" onClick={() => abrirEditarPlano(p)}>Editar</button>
                      <button className="btn-perigo btn-sm" onClick={() => desativarPlano(p.id)}>Desativar</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {/* Modal de plano */}
          {showForm && (
            <div className="overlay">
              <div className="dialog" style={{ maxWidth: 480 }}>
                <h2 style={{ fontSize: '1rem', fontWeight: 700, marginBottom: 16 }}>
                  {editando ? 'Editar Plano' : 'Novo Plano'}
                </h2>
                <form onSubmit={salvarPlano} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
                  <div className="campo">
                    <label>Nome *</label>
                    <input
                      required
                      value={formPlano.nome}
                      onChange={(e) => setFormPlano((f) => ({ ...f, nome: e.target.value }))}
                    />
                  </div>
                  <div className="campo">
                    <label>Periodicidade *</label>
                    <select
                      value={formPlano.periodicidade}
                      onChange={(e) => setFormPlano((f) => ({ ...f, periodicidade: e.target.value }))}
                    >
                      {PERIODICIDADES.map((p) => (
                        <option key={p} value={p}>{PERIODICIDADE_LABEL[p]}</option>
                      ))}
                    </select>
                  </div>
                  <div className="campo">
                    <label>Valor (R$) *</label>
                    <input
                      required
                      type="number"
                      step="0.01"
                      min="0.01"
                      value={formPlano.valor}
                      onChange={(e) => setFormPlano((f) => ({ ...f, valor: e.target.value }))}
                    />
                  </div>
                  <div className="campo">
                    <label>Descrição</label>
                    <textarea
                      rows={3}
                      value={formPlano.descricao}
                      onChange={(e) => setFormPlano((f) => ({ ...f, descricao: e.target.value }))}
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, marginTop: 8 }}>
                    <button type="button" className="btn-secundario" onClick={() => setShowForm(false)}>Cancelar</button>
                    <button type="submit" className="btn-primario">Salvar</button>
                  </div>
                </form>
              </div>
            </div>
          )}
        </>
      )}

      {/* ── TAB MATRÍCULAS ─────────────────────────────── */}
      {tab === 'matriculas' && (
        <>
          <div style={{ display: 'flex', gap: 12, alignItems: 'center', marginBottom: 16, flexWrap: 'wrap' }}>
            <div className="campo" style={{ margin: 0, minWidth: 260 }}>
              <label>Atleta</label>
              <select value={atletaSel} onChange={(e) => setAtletaSel(e.target.value)}>
                <option value="">Selecione um atleta...</option>
                {atletas.map((a) => (
                  <option key={a.id} value={a.id}>{a.nomeCompleto}</option>
                ))}
              </select>
            </div>
            <button
              className="btn-primario btn-sm"
              style={{ alignSelf: 'flex-end' }}
              onClick={() => {
                setFormMatricula({ atletaId: atletaSel, planoId: '', dataInicio: hoje(), dataFim: '', valorCustomizado: '', observacao: '' });
                setShowFormMatricula(true);
              }}
            >
              + Nova Matrícula
            </button>
          </div>

          {atletaSel && matriculas.length === 0 && (
            <p className="vazio">Nenhuma matrícula encontrada para este atleta.</p>
          )}
          {!atletaSel && <p className="vazio">Selecione um atleta para ver suas matrículas.</p>}

          {matriculas.length > 0 && (
            <table className="tabela">
              <thead>
                <tr>
                  <th>Plano</th>
                  <th>Início</th>
                  <th>Fim</th>
                  <th>Valor Efetivo</th>
                  <th>Status</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {matriculas.map((m) => (
                  <tr key={m.id}>
                    <td>{m.nomePlano}</td>
                    <td>{formatarData(m.dataInicio)}</td>
                    <td>{formatarData(m.dataFim)}</td>
                    <td>R$ {Number(m.valorEfetivo).toFixed(2).replace('.', ',')}</td>
                    <td>
                      <span style={{
                        display: 'inline-block',
                        padding: '2px 10px',
                        borderRadius: 12,
                        fontSize: '0.78rem',
                        fontWeight: 600,
                        background: m.status === 'Ativa' ? '#c6f6d5' : m.status === 'Cancelada' ? '#fed7d7' : '#fefcbf',
                        color: m.status === 'Ativa' ? '#276749' : m.status === 'Cancelada' ? '#9b2c2c' : '#744210',
                      }}>
                        {m.status}
                      </span>
                    </td>
                    <td className="acoes">
                      {m.status === 'Ativa' && (
                        <button className="btn-perigo btn-sm" onClick={() => cancelarMatricula(m.id)}>Cancelar</button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {/* Modal nova matrícula */}
          {showFormMatricula && (
            <div className="overlay">
              <div className="dialog" style={{ maxWidth: 480 }}>
                <h2 style={{ fontSize: '1rem', fontWeight: 700, marginBottom: 16 }}>Nova Matrícula</h2>
                <form onSubmit={criarMatricula} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
                  <div className="campo">
                    <label>Atleta *</label>
                    <select
                      required
                      value={formMatricula.atletaId}
                      onChange={(e) => setFormMatricula((f) => ({ ...f, atletaId: e.target.value }))}
                    >
                      <option value="">Selecione...</option>
                      {atletas.map((a) => (
                        <option key={a.id} value={a.id}>{a.nomeCompleto}</option>
                      ))}
                    </select>
                  </div>
                  <div className="campo">
                    <label>Plano *</label>
                    <select
                      required
                      value={formMatricula.planoId}
                      onChange={(e) => setFormMatricula((f) => ({ ...f, planoId: e.target.value }))}
                    >
                      <option value="">Selecione...</option>
                      {planos.map((p) => (
                        <option key={p.id} value={p.id}>
                          {p.nome} — R$ {Number(p.valor).toFixed(2).replace('.', ',')} / {p.periodicidade}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="campo">
                    <label>Data de Início *</label>
                    <input
                      required
                      type="date"
                      value={formMatricula.dataInicio}
                      onChange={(e) => setFormMatricula((f) => ({ ...f, dataInicio: e.target.value }))}
                    />
                  </div>
                  <div className="campo">
                    <label>Data de Fim (opcional)</label>
                    <input
                      type="date"
                      value={formMatricula.dataFim}
                      onChange={(e) => setFormMatricula((f) => ({ ...f, dataFim: e.target.value }))}
                    />
                  </div>
                  <div className="campo">
                    <label>Valor Customizado (opcional)</label>
                    <input
                      type="number"
                      step="0.01"
                      min="0.01"
                      placeholder="Deixe em branco para usar valor do plano"
                      value={formMatricula.valorCustomizado}
                      onChange={(e) => setFormMatricula((f) => ({ ...f, valorCustomizado: e.target.value }))}
                    />
                  </div>
                  <div className="campo">
                    <label>Observação</label>
                    <textarea
                      rows={2}
                      value={formMatricula.observacao}
                      onChange={(e) => setFormMatricula((f) => ({ ...f, observacao: e.target.value }))}
                    />
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, marginTop: 8 }}>
                    <button type="button" className="btn-secundario" onClick={() => setShowFormMatricula(false)}>Cancelar</button>
                    <button type="submit" className="btn-primario">Criar Matrícula</button>
                  </div>
                </form>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
