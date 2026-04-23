import { useEffect, useState } from 'react'
import { notificacoesApi } from '../api/notificacoesApi'

const EVENTOS_OPCOES = [
  { value: 'aniversario.atleta', label: 'Aniversário do atleta' },
  { value: 'atleta.inativo',     label: 'Atleta inativo' },
]

const VARIAVEIS = ['{NomeAtleta}', '{NomeAcademia}', '{Valor}', '{DataVencimento}']

const FORM_VAZIO = { evento: '', canal: 'Email', mensagem: '' }

export default function NotificacoesPage() {
  const [templates, setTemplates]               = useState([])
  const [form, setForm]                         = useState(FORM_VAZIO)
  const [salvando, setSalvando]                 = useState(false)
  const [editandoTemplate, setEditandoTemplate] = useState(null)
  const [alerta, setAlerta]                     = useState({ tipo: '', msg: '' })

  useEffect(() => { carregarTemplates() }, [])

  async function carregarTemplates() {
    try { setTemplates((await notificacoesApi.listarTemplates()).data) }
    catch { mostrarAlerta('error', 'Erro ao carregar templates.') }
  }

  async function salvarTemplate() {
    if (!form.evento || !form.mensagem) { mostrarAlerta('error', 'Preencha evento e mensagem.'); return }
    setSalvando(true)
    try {
      await notificacoesApi.criarTemplate(form)
      mostrarAlerta('success', 'Template criado.')
      setForm(FORM_VAZIO)
      carregarTemplates()
    } catch { mostrarAlerta('error', 'Erro ao criar template.') }
    finally { setSalvando(false) }
  }

  async function salvarEdicaoTemplate() {
    if (!editandoTemplate?.mensagem) { mostrarAlerta('error', 'Mensagem é obrigatória.'); return }
    setSalvando(true)
    try {
      await notificacoesApi.atualizarTemplate(editandoTemplate.id, {
        mensagem: editandoTemplate.mensagem,
        ativo:    editandoTemplate.ativo,
      })
      mostrarAlerta('success', 'Template atualizado.')
      setEditandoTemplate(null)
      carregarTemplates()
    } catch { mostrarAlerta('error', 'Erro ao atualizar template.') }
    finally { setSalvando(false) }
  }

  async function removerTemplate(id) {
    if (!confirm('Remover este template?')) return
    try {
      await notificacoesApi.removerTemplate(id)
      mostrarAlerta('success', 'Template removido.')
      carregarTemplates()
    } catch { mostrarAlerta('error', 'Erro ao remover template.') }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg })
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 4000)
  }

  return (
    <div className="page-container">
      <div className="page-header">
        <h2>Notificações Automáticas</h2>
      </div>

      <div style={{
        background: '#f0f9ff', border: '1px solid #bae6fd',
        borderRadius: 8, padding: '0.75rem 1rem', marginBottom: '1.5rem',
        fontSize: '0.85rem', color: '#0369a1',
      }}>
        Notificações de cobrança (geração e inadimplência) são enviadas automaticamente pelo <strong>Asaas</strong> conforme o canal configurado em cada atleta.
        Os templates abaixo são usados para eventos próprios do sistema (aniversários e inatividade).
      </div>

      {alerta.msg && <div className={`alert alert-${alerta.tipo}`}>{alerta.msg}</div>}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
        <div className="card">
          <h3>Novo Template</h3>
          <div className="form-group">
            <label>Evento</label>
            <select className="input" value={form.evento} onChange={e => setForm(f => ({ ...f, evento: e.target.value }))}>
              <option value="">Selecione</option>
              {EVENTOS_OPCOES.map(e => <option key={e.value} value={e.value}>{e.label}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label>Mensagem</label>
            <textarea className="input" rows={5} value={form.mensagem}
              onChange={e => setForm(f => ({ ...f, mensagem: e.target.value }))}
              placeholder="Olá {NomeAtleta}, parabéns pelo seu aniversário! 🥋" />
            <small style={{ color: '#6b7280' }}>Variáveis: {VARIAVEIS.join(', ')}</small>
          </div>
          <button className="btn btn-primary" onClick={salvarTemplate} disabled={salvando}>
            {salvando ? 'Salvando...' : 'Criar Template'}
          </button>
        </div>

        <div>
          <h3>Templates Configurados</h3>
          {templates.length === 0
            ? <p style={{ color: '#6b7280' }}>Nenhum template cadastrado.</p>
            : templates.map(t => (
              <div key={t.id} className="card" style={{ marginBottom: '1rem' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <strong>{EVENTOS_OPCOES.find(e => e.value === t.evento)?.label || t.evento}</strong>
                  <div style={{ display: 'flex', gap: '0.4rem', alignItems: 'center' }}>
                    <span className="badge badge-info">E-mail</span>
                    <button className="btn btn-secondary btn-sm" onClick={() => setEditandoTemplate({ id: t.id, mensagem: t.mensagem, ativo: t.ativo ?? true })}>Editar</button>
                    <button className="btn btn-danger-outline btn-sm" onClick={() => removerTemplate(t.id)}>Excluir</button>
                  </div>
                </div>
                <p style={{ marginTop: '0.5rem', fontSize: '0.875rem' }}>{t.mensagem}</p>
              </div>
            ))
          }
        </div>
      </div>

      {editandoTemplate && (
        <div className="modal-overlay" onClick={() => setEditandoTemplate(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>Editar Template</h3>
            <div className="form-group">
              <label>Mensagem</label>
              <textarea className="input" rows={5}
                value={editandoTemplate.mensagem}
                onChange={e => setEditandoTemplate(t => ({ ...t, mensagem: e.target.value }))} />
              <small style={{ color: '#6b7280' }}>Variáveis: {VARIAVEIS.join(', ')}</small>
            </div>
            <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <input type="checkbox" id="ativo-edit" checked={editandoTemplate.ativo}
                onChange={e => setEditandoTemplate(t => ({ ...t, ativo: e.target.checked }))} />
              <label htmlFor="ativo-edit">Ativo</label>
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setEditandoTemplate(null)}>Cancelar</button>
              <button className="btn btn-primary" onClick={salvarEdicaoTemplate} disabled={salvando}>
                {salvando ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
