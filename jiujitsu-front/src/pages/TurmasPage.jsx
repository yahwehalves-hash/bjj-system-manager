import { useEffect, useState } from 'react'
import { Calendar, dateFnsLocalizer } from 'react-big-calendar'
import { format, parse, startOfWeek, getDay } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import 'react-big-calendar/lib/css/react-big-calendar.css'
import { turmasApi } from '../api/turmasApi'
import { filiaisApi } from '../api/filiaisApi'

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

  const [turmas, setTurmas]       = useState([])
  const [view, setView]           = useState('week')
  const [alerta, setAlerta]       = useState({ tipo: '', msg: '' })
  const [modal, setModal]         = useState(null) // 'criar' | 'detalhe'
  const [turmaSelecionada, setTurmaSelecionada] = useState(null)
  const [form, setForm]           = useState(FORM_VAZIO)
  const [salvando, setSalvando]   = useState(false)
  const [filiais, setFiliais]     = useState([])

  useEffect(() => {
    carregar()
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
          onSelectEvent={e => { setTurmaSelecionada(e.resource); setModal('detalhe') }}
          style={{ fontFamily: 'inherit' }}
        />
      </div>

      {/* Lista compacta */}
      <h3>Todas as turmas</h3>
      <table className="table">
        <thead>
          <tr>
            <th>Nome</th><th>Dias</th><th>Horário</th><th>Alunos</th><th>Capacidade</th>
            {podeEditar && <th>Ações</th>}
          </tr>
        </thead>
        <tbody>
          {turmas.map(t => (
            <tr key={t.id}>
              <td>{t.nome}</td>
              <td>{t.diasSemana}</td>
              <td>{t.horario}</td>
              <td>{t.totalAlunos}</td>
              <td>{t.capacidadeMaxima}</td>
              {podeEditar && (
                <td>
                  <button className="btn btn-secondary btn-sm" onClick={() => abrirEdicao(t)}>Editar</button>
                  {' '}
                  <button className="btn btn-danger btn-sm" onClick={() => desativar(t.id)}>Desativar</button>
                </td>
              )}
            </tr>
          ))}
          {turmas.length === 0 && (
            <tr><td colSpan={podeEditar ? 6 : 5} style={{ textAlign: 'center' }}>Nenhuma turma cadastrada.</td></tr>
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

      {/* Modal detalhe */}
      {modal === 'detalhe' && turmaSelecionada && (
        <div className="modal-overlay" onClick={() => setModal(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>{turmaSelecionada.nome}</h3>
            <p><strong>Dias:</strong> {turmaSelecionada.diasSemana}</p>
            <p><strong>Horário:</strong> {turmaSelecionada.horario}</p>
            <p><strong>Alunos:</strong> {turmaSelecionada.totalAlunos} / {turmaSelecionada.capacidadeMaxima}</p>
            {turmaSelecionada.nomeProfessor && <p><strong>Professor:</strong> {turmaSelecionada.nomeProfessor}</p>}
            <div className="modal-actions">
              {podeEditar && <button className="btn btn-secondary" onClick={() => abrirEdicao(turmaSelecionada)}>Editar</button>}
              <button className="btn btn-primary" onClick={() => setModal(null)}>Fechar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
