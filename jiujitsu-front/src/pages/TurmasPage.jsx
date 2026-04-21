import { useEffect, useState } from 'react'
import { Calendar, dateFnsLocalizer } from 'react-big-calendar'
import { format, parse, startOfWeek, getDay } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import 'react-big-calendar/lib/css/react-big-calendar.css'
import { turmasApi } from '../api/turmasApi'
import { filiaisApi } from '../api/filiaisApi'
import { atletasApi } from '../api/atletasApi'
import { usuariosApi } from '../api/usuariosApi'

const localizer = dateFnsLocalizer({
  format,
  parse,
  startOfWeek: () => startOfWeek(new Date(), { weekStartsOn: 0 }),
  getDay,
  locales: { 'pt-BR': ptBR },
})

const DIAS_SEMANA_MAP = {
  'Domingo': 0, 'Segunda': 1, 'Terca': 2, 'Quarta': 3,
  'Quinta': 4, 'Sexta': 5, 'Sabado': 6,
}

const DIAS_OPCOES = ['Segunda', 'Terca', 'Quarta', 'Quinta', 'Sexta', 'Sabado', 'Domingo']

function turmasParaEventos(turmas) {
  const hoje = new Date()
  const eventos = []

  for (const turma of turmas) {
    const dias = turma.diasSemana?.split(',').map(d => d.trim()) ?? []
    const [hh, mm] = turma.horario?.split(':').map(Number) ?? [0, 0]

    for (const dia of dias) {
      const diaSemana = DIAS_SEMANA_MAP[dia]
      if (diaSemana === undefined) continue

      const diff   = (diaSemana - hoje.getDay() + 7) % 7
      const data   = new Date(hoje)
      data.setDate(hoje.getDate() + diff)
      data.setHours(hh, mm, 0, 0)

      const fim = new Date(data)
      fim.setHours(hh + 1, mm, 0, 0)

      eventos.push({
        id:    `${turma.id}-${dia}`,
        title: `${turma.nome} (${turma.totalAlunos}/${turma.capacidadeMaxima})`,
        start: data,
        end:   fim,
        resource: turma,
      })
    }
  }

  return eventos
}

const FORM_VAZIO = { nome: '', professorId: '', diasSemana: [], horario: '', capacidadeMaxima: 20, filialId: '' }

export default function TurmasPage({ usuario }) {
  const isAdmin      = usuario?.role === 'Admin'
  const isGestor     = usuario?.role === 'GestorFilial'
  const podeEditar   = isAdmin || isGestor

  const [turmas, setTurmas]                   = useState([])
  const [view, setView]                       = useState('week')
  const [alerta, setAlerta]                   = useState({ tipo: '', msg: '' })
  const [modal, setModal]                     = useState(null) // 'criar' | 'detalhe'
  const [turmaSelecionada, setTurmaSelecionada] = useState(null)
  const [turmaDetalhe, setTurmaDetalhe]       = useState(null) // detalhe completo com atletas
  const [form, setForm]                       = useState(FORM_VAZIO)
  const [salvando, setSalvando]               = useState(false)
  const [filiais, setFiliais]                 = useState([])
  const [todosAtletas, setTodosAtletas]       = useState([])
  const [professoresUsuario, setProfessoresUsuario] = useState([])
  const [buscaAluno, setBuscaAluno]           = useState('')
  const [vinculando, setVinculando]           = useState(false)

  useEffect(() => {
    carregar()
    carregarAtletas()
    carregarProfessores()
    if (isAdmin) filiaisApi.listar(true).then(r => setFiliais(r.data)).catch(() => {})
  }, [])

  async function carregar() {
    try {
      const res = await turmasApi.listar({ ativo: true })
      setTurmas(res.data.itens || [])
    } catch {
      mostrarAlerta('error', 'Erro ao carregar turmas.')
    }
  }

  async function carregarAtletas() {
    try {
      const res = await atletasApi.listar({ tamanhoPagina: 500 })
      setTodosAtletas(res.itens || [])
    } catch {
      // silencioso — lista de alunos fica vazia
    }
  }

  async function carregarProfessores() {
    try {
      const lista = await usuariosApi.listar()
      setProfessoresUsuario((lista || []).filter(u => u.role === 'Professor'))
    } catch {
      // silencioso
    }
  }

  async function abrirDetalhe(turma) {
    setTurmaSelecionada(turma)
    setTurmaDetalhe(null)
    setBuscaAluno('')
    setModal('detalhe')
    try {
      const res = await turmasApi.obterPorId(turma.id)
      setTurmaDetalhe(res.data)
    } catch {
      mostrarAlerta('error', 'Erro ao carregar detalhe da turma.')
    }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg })
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 3000)
  }

  async function salvar() {
    if (!form.nome || !form.horario || form.diasSemana.length === 0) {
      mostrarAlerta('error', 'Preencha nome, dias da semana e horário.')
      return
    }
    setSalvando(true)
    try {
      const payload = {
        ...form,
        diasSemana:       form.diasSemana.join(','),
        professorId:      form.professorId || null,
        capacidadeMaxima: Number(form.capacidadeMaxima),
        filialId:         isAdmin ? (form.filialId || null) : (usuario?.filialId || null),
      }
      if (turmaSelecionada) {
        await turmasApi.atualizar(turmaSelecionada.id, payload)
        mostrarAlerta('success', 'Turma atualizada.')
      } else {
        await turmasApi.criar(payload)
        mostrarAlerta('success', 'Turma criada.')
      }
      setModal(null)
      setTurmaSelecionada(null)
      setForm(FORM_VAZIO)
      carregar()
    } catch {
      mostrarAlerta('error', 'Erro ao salvar turma.')
    } finally {
      setSalvando(false)
    }
  }

  async function desativar(id) {
    if (!confirm('Desativar esta turma?')) return
    try {
      await turmasApi.desativar(id)
      mostrarAlerta('success', 'Turma desativada.')
      carregar()
    } catch {
      mostrarAlerta('error', 'Erro ao desativar turma.')
    }
  }

  function abrirEdicao(turma) {
    setTurmaSelecionada(turma)
    setForm({
      nome:             turma.nome,
      professorId:      turma.professorId || '',
      diasSemana:       turma.diasSemana?.split(',').map(d => d.trim()) ?? [],
      horario:          turma.horario,
      capacidadeMaxima: turma.capacidadeMaxima,
      filialId:         turma.filialId || '',
    })
    setModal('criar')
  }

  function toggleDia(dia) {
    setForm(f => ({
      ...f,
      diasSemana: f.diasSemana.includes(dia)
        ? f.diasSemana.filter(d => d !== dia)
        : [...f.diasSemana, dia],
    }))
  }

  async function vincularAluno(atletaId) {
    if (!turmaSelecionada) return
    setVinculando(true)
    try {
      await turmasApi.vincularAtleta(turmaSelecionada.id, atletaId)
      const res = await turmasApi.obterPorId(turmaSelecionada.id)
      setTurmaDetalhe(res.data)
      setBuscaAluno('')
      carregar()
    } catch {
      mostrarAlerta('error', 'Erro ao vincular aluno.')
    } finally {
      setVinculando(false)
    }
  }

  async function desvincularAluno(atletaId) {
    if (!turmaSelecionada) return
    try {
      await turmasApi.desvincularAtleta(turmaSelecionada.id, atletaId)
      const res = await turmasApi.obterPorId(turmaSelecionada.id)
      setTurmaDetalhe(res.data)
      carregar()
    } catch {
      mostrarAlerta('error', 'Erro ao remover aluno.')
    }
  }

  const alunosNaTurma = turmaDetalhe?.atletas ?? []
  const alunosNaTurmaIds = new Set(alunosNaTurma.map(a => a.atletaId))

  const alunosFiltrados = buscaAluno.trim().length >= 2
    ? todosAtletas.filter(a =>
        !alunosNaTurmaIds.has(a.id) &&
        (a.nomeCompleto || '').toLowerCase().includes(buscaAluno.toLowerCase())
      )
    : []

  const eventos = turmasParaEventos(turmas)

  return (
    <div className="page-container">
      <div className="page-header">
        <h2>Turmas e Horários</h2>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button className={`btn ${view === 'week' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setView('week')}>Semana</button>
          <button className={`btn ${view === 'agenda' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setView('agenda')}>Lista</button>
          {podeEditar && (
            <button className="btn btn-primary" onClick={() => { setTurmaSelecionada(null); setForm(FORM_VAZIO); setModal('criar') }}>
              + Nova Turma
            </button>
          )}
        </div>
      </div>

      {alerta.msg && <div className={`alert alert-${alerta.tipo}`}>{alerta.msg}</div>}

      <div style={{ height: 580, marginBottom: '2rem' }}>
        <Calendar
          localizer={localizer}
          events={eventos}
          view={view}
          onView={setView}
          culture="pt-BR"
          messages={{
            week: 'Semana', day: 'Dia', month: 'Mês', agenda: 'Lista',
            today: 'Hoje', previous: '‹', next: '›',
            noEventsInRange: 'Sem turmas neste período.',
          }}
          onSelectEvent={e => abrirDetalhe(e.resource)}
          style={{ fontFamily: 'inherit' }}
        />
      </div>

      {/* Lista compacta */}
      <h3>Todas as turmas</h3>
      <table className="table">
        <thead>
          <tr>
            <th>Nome</th><th>Dias</th><th>Horário</th><th>Professor</th><th>Alunos</th><th>Capacidade</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {turmas.map(t => (
            <tr key={t.id}>
              <td>{t.nome}</td>
              <td>{t.diasSemana}</td>
              <td>{t.horario}</td>
              <td>{t.nomeProfessor || <span style={{ color: '#999' }}>—</span>}</td>
              <td>{t.totalAlunos}</td>
              <td>{t.capacidadeMaxima}</td>
              <td style={{ whiteSpace: 'nowrap' }}>
                <button className="btn btn-secondary btn-sm" onClick={() => abrirDetalhe(t)}>Alunos</button>
                {podeEditar && (
                  <>
                    {' '}
                    <button className="btn btn-secondary btn-sm" onClick={() => abrirEdicao(t)}>Editar</button>
                    {' '}
                    <button className="btn btn-danger-outline btn-sm" onClick={() => desativar(t.id)}>Desativar</button>
                  </>
                )}
              </td>
            </tr>
          ))}
          {turmas.length === 0 && (
            <tr><td colSpan={7} style={{ textAlign: 'center' }}>Nenhuma turma cadastrada.</td></tr>
          )}
        </tbody>
      </table>

      {/* Modal criar/editar */}
      {modal === 'criar' && (
        <div className="modal-overlay" onClick={() => setModal(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>{turmaSelecionada ? 'Editar Turma' : 'Nova Turma'}</h3>
            {isAdmin && (
              <div className="form-group">
                <label>Filial</label>
                <select className="input" value={form.filialId} onChange={e => setForm(f => ({ ...f, filialId: e.target.value }))}>
                  <option value="">Selecione a filial</option>
                  {filiais.map(f => <option key={f.id} value={f.id}>{f.nome}</option>)}
                </select>
              </div>
            )}
            <div className="form-group">
              <label>Nome</label>
              <input className="input" value={form.nome} onChange={e => setForm(f => ({ ...f, nome: e.target.value }))} />
            </div>
            <div className="form-group">
              <label>Professor</label>
              <select
                className="input"
                value={form.professorId}
                onChange={e => setForm(f => ({ ...f, professorId: e.target.value }))}
              >
                <option value="">Sem professor definido</option>
                {professoresUsuario.map(u => (
                  <option key={u.id} value={u.id}>{u.nome}</option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Horário (HH:mm)</label>
              <input className="input" type="time" value={form.horario} onChange={e => setForm(f => ({ ...f, horario: e.target.value }))} />
            </div>
            <div className="form-group">
              <label>Capacidade máxima</label>
              <input className="input" type="number" min={1} value={form.capacidadeMaxima} onChange={e => setForm(f => ({ ...f, capacidadeMaxima: e.target.value }))} />
            </div>
            <div className="form-group">
              <label>Dias da semana</label>
              <div style={{ display: 'flex', gap: '0.4rem', flexWrap: 'wrap' }}>
                {DIAS_OPCOES.map(dia => (
                  <button
                    key={dia}
                    type="button"
                    className={`btn btn-sm ${form.diasSemana.includes(dia) ? 'btn-primary' : 'btn-secondary'}`}
                    onClick={() => toggleDia(dia)}
                  >
                    {dia}
                  </button>
                ))}
              </div>
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setModal(null)}>Cancelar</button>
              <button className="btn btn-primary" onClick={salvar} disabled={salvando}>
                {salvando ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal detalhe com gestão de alunos */}
      {modal === 'detalhe' && turmaSelecionada && (
        <div className="modal-overlay" onClick={() => setModal(null)}>
          <div className="modal" style={{ maxWidth: 560 }} onClick={e => e.stopPropagation()}>
            <h3>{turmaSelecionada.nome}</h3>
            <p><strong>Dias:</strong> {turmaSelecionada.diasSemana}</p>
            <p><strong>Horário:</strong> {turmaSelecionada.horario}</p>
            <p><strong>Capacidade:</strong> {turmaSelecionada.totalAlunos} / {turmaSelecionada.capacidadeMaxima}</p>
            {(turmaDetalhe?.nomeProfessor || turmaSelecionada.nomeProfessor) && (
              <p><strong>Professor:</strong> {turmaDetalhe?.nomeProfessor ?? turmaSelecionada.nomeProfessor}</p>
            )}

            <hr style={{ margin: '1rem 0' }} />
            <h4 style={{ marginBottom: '0.5rem' }}>Alunos matriculados</h4>

            {turmaDetalhe === null ? (
              <p style={{ color: '#999' }}>Carregando...</p>
            ) : alunosNaTurma.length === 0 ? (
              <p style={{ color: '#999' }}>Nenhum aluno vinculado.</p>
            ) : (
              <ul style={{ listStyle: 'none', padding: 0, margin: '0 0 0.75rem', maxHeight: 180, overflowY: 'auto' }}>
                {alunosNaTurma.map(a => (
                  <li key={a.atletaId} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '0.3rem 0', borderBottom: '1px solid #eee' }}>
                    <span>{a.nomeAtleta}</span>
                    {podeEditar && (
                      <button className="btn btn-danger-outline btn-sm" onClick={() => desvincularAluno(a.atletaId)}>Remover</button>
                    )}
                  </li>
                ))}
              </ul>
            )}

            {podeEditar && (
              <div className="form-group">
                <label>Adicionar aluno</label>
                <input
                  className="input"
                  placeholder="Digite o nome (mín. 2 letras)..."
                  value={buscaAluno}
                  onChange={e => setBuscaAluno(e.target.value)}
                />
                {alunosFiltrados.length > 0 && (
                  <ul style={{ listStyle: 'none', padding: 0, margin: '0.25rem 0 0', border: '1px solid #ddd', borderRadius: 4, maxHeight: 160, overflowY: 'auto' }}>
                    {alunosFiltrados.map(a => (
                      <li
                        key={a.id}
                        style={{ padding: '0.4rem 0.75rem', cursor: 'pointer', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
                        onMouseEnter={e => e.currentTarget.style.background = '#f5f5f5'}
                        onMouseLeave={e => e.currentTarget.style.background = ''}
                      >
                        <span>{a.nomeCompleto}</span>
                        <button
                          className="btn btn-primary btn-sm"
                          disabled={vinculando}
                          onClick={() => vincularAluno(a.id)}
                        >
                          + Adicionar
                        </button>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            )}

            <div className="modal-actions">
              {podeEditar && <button className="btn btn-secondary" onClick={() => abrirEdicao(turmaSelecionada)}>Editar turma</button>}
              <button className="btn btn-primary" onClick={() => setModal(null)}>Fechar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
