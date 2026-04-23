import { useEffect, useState } from 'react'
import { turmasApi } from '../api/turmasApi'
import { presencaApi } from '../api/presencaApi'
import { CheckSquare, Square, Users, BarChart2, ChevronDown, ChevronUp } from 'lucide-react'

function hoje() {
  return new Date().toISOString().split('T')[0]
}

function inicioMes() {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`
}

function formatarPct(pct) {
  const n = Number(pct)
  const cor = n >= 75 ? '#22c55e' : n >= 50 ? '#f59e0b' : '#ef4444'
  return <span style={{ color: cor, fontWeight: 600 }}>{n.toFixed(1)}%</span>
}

export default function PresencaPage({ usuario }) {
  const [turmas,          setTurmas]          = useState([])
  const [turmaSelecionada, setTurmaSelecionada] = useState(null)
  const [atletas,          setAtletas]          = useState([])
  const [marcados,         setMarcados]         = useState(new Set())
  const [salvando,         setSalvando]         = useState(false)
  const [aba,              setAba]              = useState('chamada') // 'chamada' | 'frequencia'
  const [frequencia,       setFrequencia]       = useState([])
  const [dataInicio,       setDataInicio]       = useState(inicioMes())
  const [dataFim,          setDataFim]          = useState(hoje())
  const [carregando,       setCarregando]       = useState(false)
  const [feedback,         setFeedback]         = useState(null)

  useEffect(() => {
    turmasApi.listar({ ativo: true }).then(r => setTurmas(r.data.itens ?? []))
  }, [])

  async function selecionarTurma(turma) {
    setTurmaSelecionada(turma)
    setMarcados(new Set())
    setFrequencia([])

    const detalhe = await turmasApi.obterPorId(turma.id)
    setAtletas(detalhe.data.atletas ?? [])
  }

  function toggleAtleta(id) {
    setMarcados(prev => {
      const novo = new Set(prev)
      novo.has(id) ? novo.delete(id) : novo.add(id)
      return novo
    })
  }

  function toggleTodos() {
    if (marcados.size === atletas.length) {
      setMarcados(new Set())
    } else {
      setMarcados(new Set(atletas.map(a => a.atletaId)))
    }
  }

  async function salvarChamada() {
    if (!turmaSelecionada || marcados.size === 0) return
    setSalvando(true)
    setFeedback(null)
    try {
      const resp = await presencaApi.registrarEmLote({
        turmaId:  turmaSelecionada.id,
        filialId: turmaSelecionada.filialId,
        atletaIds: [...marcados],
      })
      setFeedback({ tipo: 'ok', msg: `${resp.data.registrados} presença(s) registrada(s).` })
      setMarcados(new Set())
    } catch (e) {
      setFeedback({ tipo: 'erro', msg: e.response?.data?.erro ?? 'Erro ao salvar chamada.' })
    } finally {
      setSalvando(false)
    }
  }

  async function carregarFrequencia() {
    if (!turmaSelecionada) return
    setCarregando(true)
    try {
      const resp = await presencaApi.frequenciaTurma(
        turmaSelecionada.id, dataInicio, dataFim)
      setFrequencia(resp.data ?? [])
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    if (aba === 'frequencia' && turmaSelecionada) carregarFrequencia()
  }, [aba, turmaSelecionada])

  return (
    <div style={{ padding: '1.5rem', maxWidth: 900 }}>
      <h2 style={{ marginBottom: '1.5rem', fontSize: '1.25rem', fontWeight: 700 }}>
        Controle de Presença
      </h2>

      {/* Seleção de turma */}
      <div style={{ marginBottom: '1.5rem' }}>
        <label style={{ display: 'block', fontWeight: 600, marginBottom: 6 }}>Turma</label>
        <select
          style={{ padding: '0.5rem 0.75rem', borderRadius: 6, border: '1px solid #d1d5db', width: '100%', maxWidth: 400 }}
          value={turmaSelecionada?.id ?? ''}
          onChange={e => {
            const t = turmas.find(x => x.id === e.target.value)
            if (t) selecionarTurma(t)
          }}
        >
          <option value="">Selecione uma turma...</option>
          {turmas.map(t => (
            <option key={t.id} value={t.id}>
              {t.nome} — {t.diasSemana} {t.horario}
            </option>
          ))}
        </select>
      </div>

      {turmaSelecionada && (
        <>
          {/* Abas */}
          <div style={{ display: 'flex', gap: 8, marginBottom: '1.25rem', borderBottom: '1px solid #e5e7eb' }}>
            {[
              { key: 'chamada',    label: 'Chamada do Dia', icon: <Users size={15} /> },
              { key: 'frequencia', label: 'Frequência',     icon: <BarChart2 size={15} /> },
            ].map(a => (
              <button
                key={a.key}
                onClick={() => setAba(a.key)}
                style={{
                  display: 'flex', alignItems: 'center', gap: 6,
                  padding: '0.5rem 1rem', border: 'none', background: 'none',
                  borderBottom: aba === a.key ? '2px solid #6366f1' : '2px solid transparent',
                  color: aba === a.key ? '#6366f1' : '#6b7280',
                  fontWeight: aba === a.key ? 700 : 400,
                  cursor: 'pointer', marginBottom: -1,
                }}
              >
                {a.icon} {a.label}
              </button>
            ))}
          </div>

          {/* === ABA CHAMADA === */}
          {aba === 'chamada' && (
            <div>
              {feedback && (
                <div style={{
                  padding: '0.75rem 1rem', borderRadius: 6, marginBottom: '1rem',
                  background: feedback.tipo === 'ok' ? '#dcfce7' : '#fee2e2',
                  color: feedback.tipo === 'ok' ? '#16a34a' : '#dc2626',
                }}>
                  {feedback.msg}
                </div>
              )}

              {atletas.length === 0 ? (
                <p style={{ color: '#6b7280' }}>Nenhum atleta vinculado a esta turma.</p>
              ) : (
                <>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                    <span style={{ fontSize: '0.875rem', color: '#6b7280' }}>
                      {marcados.size} de {atletas.length} marcados
                    </span>
                    <button
                      onClick={toggleTodos}
                      style={{ fontSize: '0.8rem', color: '#6366f1', background: 'none', border: 'none', cursor: 'pointer' }}
                    >
                      {marcados.size === atletas.length ? 'Desmarcar todos' : 'Marcar todos'}
                    </button>
                  </div>

                  <div style={{ display: 'flex', flexDirection: 'column', gap: 8, marginBottom: '1.25rem' }}>
                    {atletas.map(a => (
                      <div
                        key={a.atletaId}
                        onClick={() => toggleAtleta(a.atletaId)}
                        style={{
                          display: 'flex', alignItems: 'center', gap: 12,
                          padding: '0.625rem 0.875rem', borderRadius: 8,
                          border: '1px solid',
                          borderColor: marcados.has(a.atletaId) ? '#6366f1' : '#e5e7eb',
                          background: marcados.has(a.atletaId) ? '#eef2ff' : '#fff',
                          cursor: 'pointer', userSelect: 'none',
                        }}
                      >
                        {marcados.has(a.atletaId)
                          ? <CheckSquare size={18} color="#6366f1" />
                          : <Square size={18} color="#9ca3af" />}
                        <span style={{ fontWeight: 500 }}>{a.nomeAtleta}</span>
                        <span style={{
                          marginLeft: 'auto', fontSize: '0.75rem',
                          color: '#9ca3af', textTransform: 'capitalize',
                        }}>
                          {a.faixa?.toLowerCase()} — {a.grau}° grau
                        </span>
                      </div>
                    ))}
                  </div>

                  <button
                    onClick={salvarChamada}
                    disabled={salvando || marcados.size === 0}
                    style={{
                      padding: '0.625rem 1.5rem', borderRadius: 6,
                      background: marcados.size === 0 ? '#e5e7eb' : '#6366f1',
                      color: marcados.size === 0 ? '#9ca3af' : '#fff',
                      border: 'none', fontWeight: 600, cursor: marcados.size === 0 ? 'not-allowed' : 'pointer',
                    }}
                  >
                    {salvando ? 'Salvando...' : `Salvar Chamada (${marcados.size})`}
                  </button>
                </>
              )}
            </div>
          )}

          {/* === ABA FREQUENCIA === */}
          {aba === 'frequencia' && (
            <div>
              {/* Filtro de período */}
              <div style={{ display: 'flex', gap: 12, alignItems: 'flex-end', marginBottom: '1.25rem', flexWrap: 'wrap' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', color: '#6b7280', marginBottom: 4 }}>De</label>
                  <input type="date" value={dataInicio} onChange={e => setDataInicio(e.target.value)}
                    style={{ padding: '0.45rem 0.75rem', borderRadius: 6, border: '1px solid #d1d5db' }} />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', color: '#6b7280', marginBottom: 4 }}>Até</label>
                  <input type="date" value={dataFim} onChange={e => setDataFim(e.target.value)}
                    style={{ padding: '0.45rem 0.75rem', borderRadius: 6, border: '1px solid #d1d5db' }} />
                </div>
                <button
                  onClick={carregarFrequencia}
                  style={{ padding: '0.5rem 1.25rem', borderRadius: 6, background: '#6366f1', color: '#fff', border: 'none', fontWeight: 600, cursor: 'pointer' }}
                >
                  Filtrar
                </button>
              </div>

              {carregando ? (
                <p style={{ color: '#6b7280' }}>Carregando...</p>
              ) : frequencia.length === 0 ? (
                <p style={{ color: '#6b7280' }}>Nenhum registro no período.</p>
              ) : (
                <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
                  <thead>
                    <tr style={{ borderBottom: '2px solid #e5e7eb', textAlign: 'left' }}>
                      <th style={{ padding: '0.5rem 0.75rem' }}>Atleta</th>
                      <th style={{ padding: '0.5rem 0.75rem', textAlign: 'center' }}>Presenças</th>
                      <th style={{ padding: '0.5rem 0.75rem', textAlign: 'center' }}>Aulas</th>
                      <th style={{ padding: '0.5rem 0.75rem', textAlign: 'center' }}>Frequência</th>
                      <th style={{ padding: '0.5rem 0.75rem' }}>Última Presença</th>
                    </tr>
                  </thead>
                  <tbody>
                    {frequencia.map(f => (
                      <tr key={f.atletaId} style={{ borderBottom: '1px solid #f3f4f6' }}>
                        <td style={{ padding: '0.5rem 0.75rem', fontWeight: 500 }}>{f.nomeAtleta}</td>
                        <td style={{ padding: '0.5rem 0.75rem', textAlign: 'center' }}>{f.totalPresencas}</td>
                        <td style={{ padding: '0.5rem 0.75rem', textAlign: 'center' }}>{f.totalAulas}</td>
                        <td style={{ padding: '0.5rem 0.75rem', textAlign: 'center' }}>{formatarPct(f.percentualFrequencia)}</td>
                        <td style={{ padding: '0.5rem 0.75rem', color: '#6b7280' }}>
                          {f.ultimaPresenca ? new Date(f.ultimaPresenca).toLocaleDateString('pt-BR') : '—'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </>
      )}
    </div>
  )
}
