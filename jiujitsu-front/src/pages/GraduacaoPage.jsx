import { useEffect, useState } from 'react'
import { graduacaoApi } from '../api/graduacaoApi'

const FAIXAS = ['Branca', 'Cinza', 'Azul', 'Roxa', 'Marrom', 'Preta']
const FAIXA_ORDEM = Object.fromEntries(FAIXAS.map((f, i) => [f, i]))

const FAIXAS_ENUM = { Branca: 1, Cinza: 2, Azul: 3, Roxa: 4, Marrom: 5, Preta: 6 }
const FAIXAS_CORES = {
  Branca: '#e0e0e0', Cinza: '#9e9e9e', Azul: '#1565c0',
  Roxa: '#6a1b9a', Marrom: '#5d4037', Preta: '#212121',
}

function grausParaFaixa(faixa) {
  return faixa === 'Preta' ? [0, 1, 2, 3, 4, 5, 6, 7] : [0, 1, 2, 3, 4]
}

function formatarMeses(meses) {
  if (meses == null) return '—'
  if (meses === 0) return '0 meses'
  const anos = Math.floor(meses / 12)
  const resto = meses % 12
  if (anos === 0) return `${meses} ${meses === 1 ? 'mês' : 'meses'}`
  if (resto === 0) return anos === 1 ? '1 ano' : `${anos} anos`
  return `${anos} ${anos === 1 ? 'ano' : 'anos'} e ${resto} ${resto === 1 ? 'mês' : 'meses'}`
}

function BadgeFaixa({ faixa, grau }) {
  const cor = FAIXAS_CORES[faixa] ?? '#ccc'
  const corTexto = ['Branca', 'Cinza'].includes(faixa) ? '#111' : '#fff'
  return (
    <span style={{
      background: cor, color: corTexto,
      padding: '2px 10px', borderRadius: 4, fontWeight: 600, fontSize: '0.85rem',
    }}>
      {faixa}{grau > 0 ? ` (${grau}° grau)` : ''}
    </span>
  )
}

const REGRA_NOVA_VAZIO = { faixa: '', grau: '', tempo: '' }

export default function GraduacaoPage({ usuario }) {
  const isAdmin = usuario?.role === 'Admin'
  const [aba, setAba]             = useState('elegiveis')
  const [elegiveis, setElegiveis] = useState([])
  const [regras, setRegras]       = useState([])
  const [alerta, setAlerta]       = useState({ tipo: '', msg: '' })
  const [salvando, setSalvando]   = useState(false)

  // Estado separado para edição de regra existente
  const [editandoRegra, setEditandoRegra] = useState(null)
  // Estado para nova regra — completamente independente
  const [novaRegra, setNovaRegra] = useState(REGRA_NOVA_VAZIO)

  useEffect(() => {
    if (aba === 'elegiveis') carregarElegiveis()
    if (aba === 'regras')    carregarRegras()
  }, [aba])

  async function carregarElegiveis() {
    try {
      const res = await graduacaoApi.listarElegiveis()
      const ordenados = [...res.data].sort((a, b) => {
        const dif = (FAIXA_ORDEM[a.faixaAtual] ?? 99) - (FAIXA_ORDEM[b.faixaAtual] ?? 99)
        return dif !== 0 ? dif : (a.grauAtual ?? 0) - (b.grauAtual ?? 0)
      })
      setElegiveis(ordenados)
    } catch {
      mostrarAlerta('error', 'Erro ao carregar elegíveis.')
    }
  }

  async function carregarRegras() {
    try {
      const res = await graduacaoApi.listarRegras()
      const ordenadas = [...res.data].sort((a, b) => {
        const dif = (FAIXA_ORDEM[a.faixa] ?? 99) - (FAIXA_ORDEM[b.faixa] ?? 99)
        return dif !== 0 ? dif : (a.grau ?? 0) - (b.grau ?? 0)
      })
      setRegras(ordenadas)
    } catch {
      mostrarAlerta('error', 'Erro ao carregar regras.')
    }
  }

  async function salvarEdicao() {
    if (!editandoRegra) return
    setSalvando(true)
    try {
      await graduacaoApi.salvarRegra({
        filialId:         null,
        faixa:            FAIXAS_ENUM[editandoRegra.faixa],
        grau:             Number(editandoRegra.grau),
        tempoMinimoMeses: Number(editandoRegra.tempo),
      })
      mostrarAlerta('success', 'Regra atualizada.')
      setEditandoRegra(null)
      carregarRegras()
    } catch {
      mostrarAlerta('error', 'Erro ao salvar regra.')
    } finally {
      setSalvando(false)
    }
  }

  async function salvarNovaRegra() {
    if (!novaRegra.faixa || novaRegra.grau === '' || novaRegra.tempo === '') {
      mostrarAlerta('error', 'Preencha faixa, grau e tempo mínimo.')
      return
    }
    setSalvando(true)
    try {
      await graduacaoApi.salvarRegra({
        filialId:         null,
        faixa:            FAIXAS_ENUM[novaRegra.faixa],
        grau:             Number(novaRegra.grau),
        tempoMinimoMeses: Number(novaRegra.tempo),
      })
      mostrarAlerta('success', 'Regra adicionada.')
      setNovaRegra(REGRA_NOVA_VAZIO)
      carregarRegras()
    } catch {
      mostrarAlerta('error', 'Erro ao adicionar regra.')
    } finally {
      setSalvando(false)
    }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg })
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 3500)
  }

  return (
    <div className="page-container">
      <div className="page-header">
        <h2>Programa de Graduação</h2>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button className={`btn ${aba === 'elegiveis' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setAba('elegiveis')}>
            Elegíveis
          </button>
          {isAdmin && (
            <button className={`btn ${aba === 'regras' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setAba('regras')}>
              Regras
            </button>
          )}
        </div>
      </div>

      {alerta.msg && <div className={`alert alert-${alerta.tipo}`}>{alerta.msg}</div>}

      {/* ── Aba Elegíveis ─────────────────────────────── */}
      {aba === 'elegiveis' && (
        <>
          <p style={{ color: '#6b7280', marginBottom: '1rem' }}>
            Atletas que atingiram o tempo mínimo configurado na faixa/grau atual.
          </p>
          {elegiveis.length === 0 ? (
            <div style={{
              background: '#fffbeb', border: '1px solid #fcd34d', borderRadius: 8,
              padding: '1rem', color: '#92400e', fontSize: '0.9rem',
            }}>
              Nenhum atleta elegível no momento.
              {isAdmin && ' Certifique-se de que as regras de graduação foram configuradas na aba "Regras".'}
            </div>
          ) : (
            <table className="table">
              <thead>
                <tr>
                  <th>Atleta</th><th>Filial</th><th>Faixa/Grau Atual</th>
                  <th>Tempo na Faixa</th><th>Mínimo Exigido</th>
                </tr>
              </thead>
              <tbody>
                {elegiveis.map(e => (
                  <tr key={e.atletaId}>
                    <td>{e.nomeAtleta}</td>
                    <td>{e.nomeFilial}</td>
                    <td><BadgeFaixa faixa={e.faixaAtual} grau={e.grauAtual} /></td>
                    <td>{formatarMeses(e.mesesNaFaixa)}</td>
                    <td>{formatarMeses(e.tempoMinimoNecessario)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </>
      )}

      {/* ── Aba Regras ────────────────────────────────── */}
      {aba === 'regras' && isAdmin && (
        <>
          <p style={{ color: '#6b7280', marginBottom: '1rem' }}>
            Configure o tempo mínimo para progressão em cada faixa/grau.
            A faixa Preta possui graus de 0 a 7.
          </p>
          <table className="table">
            <thead>
              <tr><th>Faixa</th><th>Grau</th><th>Tempo Mínimo</th><th>Ação</th></tr>
            </thead>
            <tbody>
              {regras.map(r => {
                const editando = editandoRegra?.id === r.id
                return (
                  <tr key={r.id}>
                    <td><BadgeFaixa faixa={r.faixa} grau={0} /></td>
                    <td>
                      {editando ? (
                        <select
                          className="input"
                          style={{ width: 70 }}
                          value={editandoRegra.grau}
                          onChange={e => setEditandoRegra(ed => ({ ...ed, grau: Number(e.target.value) }))}
                        >
                          {grausParaFaixa(editandoRegra.faixa).map(g => (
                            <option key={g} value={g}>{g}</option>
                          ))}
                        </select>
                      ) : r.grau}
                    </td>
                    <td>
                      {editando ? (
                        <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                          <input
                            className="input"
                            type="number"
                            min={0}
                            style={{ width: 90 }}
                            value={editandoRegra.tempo}
                            onChange={e => setEditandoRegra(ed => ({ ...ed, tempo: e.target.value }))}
                          />
                          <span style={{ fontSize: '0.78rem', color: '#6b7280' }}>
                            {formatarMeses(Number(editandoRegra.tempo))}
                          </span>
                        </div>
                      ) : formatarMeses(r.tempoMinimoMeses)}
                    </td>
                    <td>
                      {editando ? (
                        <>
                          <button className="btn btn-primary btn-sm" onClick={salvarEdicao} disabled={salvando}>Salvar</button>
                          {' '}
                          <button className="btn btn-secondary btn-sm" onClick={() => setEditandoRegra(null)}>Cancelar</button>
                        </>
                      ) : (
                        <button
                          className="btn btn-secondary btn-sm"
                          onClick={() => setEditandoRegra({ id: r.id, faixa: r.faixa, grau: r.grau, tempo: r.tempoMinimoMeses })}
                        >
                          Editar
                        </button>
                      )}
                    </td>
                  </tr>
                )
              })}
              {regras.length === 0 && (
                <tr>
                  <td colSpan={4} style={{ textAlign: 'center', color: '#9ca3af', padding: '1.5rem' }}>
                    Nenhuma regra configurada ainda.
                  </td>
                </tr>
              )}
            </tbody>
          </table>

          {/* Formulário nova regra — estado independente */}
          <div style={{
            marginTop: '1.5rem', background: '#f9fafb',
            border: '1px solid #e5e7eb', borderRadius: 8, padding: '1rem',
          }}>
            <h4 style={{ marginBottom: '0.75rem', fontSize: '0.95rem' }}>Adicionar nova regra</h4>
            <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
              <div className="form-group" style={{ marginBottom: 0 }}>
                <label>Faixa</label>
                <select
                  className="input"
                  value={novaRegra.faixa}
                  onChange={e => setNovaRegra(r => ({ ...r, faixa: e.target.value, grau: '' }))}
                >
                  <option value="">Selecione</option>
                  {FAIXAS.map(f => <option key={f} value={f}>{f}</option>)}
                </select>
              </div>
              <div className="form-group" style={{ marginBottom: 0 }}>
                <label>Grau</label>
                <select
                  className="input"
                  value={novaRegra.grau}
                  onChange={e => setNovaRegra(r => ({ ...r, grau: e.target.value }))}
                  disabled={!novaRegra.faixa}
                >
                  <option value="">Selecione</option>
                  {grausParaFaixa(novaRegra.faixa).map(g => (
                    <option key={g} value={g}>{g === 0 ? '0 (sem grau)' : g}</option>
                  ))}
                </select>
              </div>
              <div className="form-group" style={{ marginBottom: 0 }}>
                <label>Meses mínimos</label>
                <input
                  className="input"
                  type="number"
                  min={0}
                  style={{ width: 110 }}
                  placeholder="ex: 24"
                  value={novaRegra.tempo}
                  onChange={e => setNovaRegra(r => ({ ...r, tempo: e.target.value }))}
                />
              </div>
              {novaRegra.tempo !== '' && (
                <span style={{ fontSize: '0.8rem', color: '#6b7280', alignSelf: 'center' }}>
                  = {formatarMeses(Number(novaRegra.tempo))}
                </span>
              )}
              <button className="btn btn-primary" onClick={salvarNovaRegra} disabled={salvando}>
                {salvando ? 'Salvando...' : 'Adicionar'}
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
