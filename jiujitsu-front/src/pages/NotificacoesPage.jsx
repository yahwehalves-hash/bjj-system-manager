import { useEffect, useState } from 'react'
import { notificacoesApi } from '../api/notificacoesApi'

const EVENTOS_OPCOES = [
  { value: 'mensalidade.vencendo',     label: 'Mensalidade vencendo (D-3)' },
  { value: 'mensalidade.vencida',      label: 'Mensalidade vencida (D0)' },
  { value: 'mensalidade.inadimplente', label: 'Mensalidade inadimplente (D+7)' },
  { value: 'aniversario.atleta',       label: 'Aniversário do atleta' },
]
const CANAIS    = ['WhatsApp', 'Email']
const VARIAVEIS = ['{NomeAtleta}', '{NomeAcademia}', '{Valor}', '{DataVencimento}']
const FORM_VAZIO = { evento: '', canal: 'WhatsApp', mensagem: '' }

function StatusBadge({ estado }) {
  const cfg = {
    open:                   { bg: '#c6f6d5', cor: '#276749', label: 'Conectado ✓' },
    connecting:             { bg: '#fefcbf', cor: '#744210', label: 'Conectando...' },
    close:                  { bg: '#fed7d7', cor: '#9b2c2c', label: 'Desconectado' },
    NAO_CONFIGURADO:        { bg: '#e2e8f0', cor: '#4a5568', label: 'Não configurado' },
    EVOLUTION_INDISPONIVEL: { bg: '#fed7d7', cor: '#9b2c2c', label: 'Evolution API offline' },
    ERRO:                   { bg: '#fed7d7', cor: '#9b2c2c', label: 'Erro' },
  }
  const s = cfg[estado] ?? cfg.ERRO
  return (
    <span style={{
      display: 'inline-block', padding: '3px 14px', borderRadius: 12,
      fontSize: '0.82rem', fontWeight: 600, background: s.bg, color: s.cor,
    }}>
      {s.label}
    </span>
  )
}

export default function NotificacoesPage() {
  const [aba, setAba] = useState('templates')

  const [templates, setTemplates]         = useState([])
  const [form, setForm]                   = useState(FORM_VAZIO)
  const [salvando, setSalvando]           = useState(false)
  const [editandoTemplate, setEditandoTemplate] = useState(null) // { id, mensagem, ativo }

  const [wppConfig, setWppConfig]   = useState(null)
  const [wppEstado, setWppEstado]   = useState(null)
  const [qrcode, setQrcode]         = useState(null)
  const [conectando, setConectando] = useState(false)
  const [testeFone, setTesteFone]   = useState('')
  const [testeMsg, setTesteMsg]     = useState('Olá! Esta é uma mensagem de teste da Academia. 🥋')
  const [enviando, setEnviando]     = useState(false)

  const [alerta, setAlerta] = useState({ tipo: '', msg: '' })

  useEffect(() => {
    if (aba === 'templates') carregarTemplates()
    if (aba === 'whatsapp')  carregarWhatsApp()
  }, [aba])

  async function carregarTemplates() {
    try { setTemplates((await notificacoesApi.listarTemplates()).data) }
    catch { mostrarAlerta('error', 'Erro ao carregar templates.') }
  }

  async function carregarWhatsApp() {
    try {
      const [cfg, status] = await Promise.all([
        notificacoesApi.whatsappConfig(),
        notificacoesApi.whatsappStatus(),
      ])
      setWppConfig(cfg.data)
      setWppEstado(status.data)
    } catch { mostrarAlerta('error', 'Erro ao verificar Evolution API.') }
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

  async function conectarWhatsApp() {
    setConectando(true)
    setQrcode(null)
    try {
      const { data } = await notificacoesApi.whatsappConectar()
      const raw = data?.qrcode?.base64 || data?.base64 || data?.code || null
      if (raw) {
        setQrcode(raw.startsWith('data:') ? raw : `data:image/png;base64,${raw}`)
        mostrarAlerta('success', 'QR Code gerado! Escaneie com o WhatsApp.')
      } else {
        mostrarAlerta('error', 'QR Code não encontrado. Instância pode já estar conectada.')
      }
      carregarWhatsApp()
    } catch (err) {
      mostrarAlerta('error', err.response?.data?.erro || 'Erro ao conectar. Verifique se o container Evolution está rodando no Aspire.')
    } finally { setConectando(false) }
  }

  async function atualizarStatus() {
    try { setWppEstado((await notificacoesApi.whatsappStatus()).data) }
    catch { mostrarAlerta('error', 'Erro ao verificar status.') }
  }

  async function enviarTeste() {
    if (!testeFone || !testeMsg) { mostrarAlerta('error', 'Informe telefone e mensagem.'); return }
    setEnviando(true)
    try {
      await notificacoesApi.whatsappTestar({ telefone: testeFone, mensagem: testeMsg })
      mostrarAlerta('success', `Mensagem enviada para ${testeFone}!`)
    } catch (err) {
      mostrarAlerta('error', err.response?.data?.erro || 'Falha ao enviar.')
    } finally { setEnviando(false) }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg })
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 4000)
  }

  const estadoConexao = wppEstado?.instance?.state
    || wppEstado?.state
    || wppEstado?.estado
    || (wppConfig?.configurado === false ? 'NAO_CONFIGURADO' : 'close')
  const conectado = estadoConexao === 'open'

  return (
    <div className="page-container">
      <div className="page-header">
        <h2>Notificações Automáticas</h2>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          {[{ id: 'templates', label: 'Templates' }, { id: 'whatsapp', label: '📱 WhatsApp' }].map(t => (
            <button key={t.id} className={`btn ${aba === t.id ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setAba(t.id)}>
              {t.label}
            </button>
          ))}
        </div>
      </div>

      {alerta.msg && <div className={`alert alert-${alerta.tipo}`}>{alerta.msg}</div>}

      {/* ── Templates ─────────────────────────────────── */}
      {aba === 'templates' && (
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
              <label>Canal</label>
              <select className="input" value={form.canal} onChange={e => setForm(f => ({ ...f, canal: e.target.value }))}>
                {CANAIS.map(c => <option key={c} value={c}>{c}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label>Mensagem</label>
              <textarea className="input" rows={5} value={form.mensagem}
                onChange={e => setForm(f => ({ ...f, mensagem: e.target.value }))}
                placeholder="Olá {NomeAtleta}, sua mensalidade de {Valor} vence em {DataVencimento}." />
              <small style={{ color: '#6b7280' }}>Variáveis: {VARIAVEIS.join(', ')}</small>
            </div>
            <button className="btn btn-primary" onClick={salvarTemplate} disabled={salvando}>
              {salvando ? 'Salvando...' : 'Criar Template'}
            </button>
          </div>

          <div>
            <h3>Templates Configurados</h3>
            {templates.length === 0 ? <p style={{ color: '#6b7280' }}>Nenhum template.</p>
              : templates.map(t => (
                <div key={t.id} className="card" style={{ marginBottom: '1rem' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <strong>{EVENTOS_OPCOES.find(e => e.value === t.evento)?.label || t.evento}</strong>
                    <div style={{ display: 'flex', gap: '0.4rem', alignItems: 'center' }}>
                      <span className={`badge ${t.canal === 'WhatsApp' ? 'badge-success' : 'badge-info'}`}>{t.canal}</span>
                      <button className="btn btn-secondary btn-sm" onClick={() => setEditandoTemplate({ id: t.id, mensagem: t.mensagem, ativo: t.ativo ?? true })}>Editar</button>
                      <button className="btn btn-danger btn-sm" onClick={() => removerTemplate(t.id)}>Excluir</button>
                    </div>
                  </div>
                  <p style={{ marginTop: '0.5rem', fontSize: '0.875rem' }}>{t.mensagem}</p>
                </div>
              ))}
          </div>
        </div>
      )}

      {/* ── WhatsApp ──────────────────────────────────── */}
      {/* ── Modal editar template ─────────────────────── */}
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

      {aba === 'whatsapp' && (
        <div style={{ maxWidth: 680 }}>

          <div className="card" style={{ marginBottom: '1.5rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
              <h3 style={{ margin: 0 }}>Status da Conexão</h3>
              <button className="btn btn-secondary btn-sm" onClick={atualizarStatus}>↻ Atualizar</button>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 12 }}>
              <StatusBadge estado={estadoConexao} />
              {wppConfig && (
                <span style={{ fontSize: '0.82rem', color: '#6b7280' }}>
                  Instância: <strong>{wppConfig.instance}</strong> · {wppConfig.baseUrl}
                </span>
              )}
            </div>
            {!conectado ? (
              <div>
                <p style={{ fontSize: '0.88rem', color: '#6b7280', marginBottom: 12 }}>
                  Clique em <strong>"Gerar QR Code"</strong> e escaneie no celular em
                  {' '}<strong>WhatsApp → Aparelhos conectados → Conectar aparelho</strong>.
                </p>
                <button className="btn btn-primary" onClick={conectarWhatsApp} disabled={conectando}>
                  {conectando ? 'Aguarde...' : '📷 Gerar QR Code'}
                </button>
              </div>
            ) : (
              <p style={{ color: '#276749', fontWeight: 500 }}>WhatsApp conectado. Envie mensagens abaixo.</p>
            )}
          </div>

          {qrcode && !conectado && (
            <div className="card" style={{ marginBottom: '1.5rem', textAlign: 'center' }}>
              <h4 style={{ marginBottom: 16 }}>Escaneie com o WhatsApp do seu celular</h4>
              <img src={qrcode} alt="QR Code WhatsApp"
                style={{ width: 260, height: 260, border: '1px solid #e5e7eb', borderRadius: 8 }} />
              <p style={{ marginTop: 12, fontSize: '0.82rem', color: '#6b7280' }}>
                O QR Code expira em ~60 segundos. Após escanear, clique em "Atualizar".
              </p>
            </div>
          )}

          <div className="card">
            <h3>Enviar Mensagem de Teste</h3>
            <p style={{ fontSize: '0.85rem', color: '#6b7280', marginBottom: 16 }}>
              Formato: DDI + DDD + número (somente dígitos). Ex: <code>5511999999999</code>
            </p>
            <div className="form-group">
              <label>Telefone</label>
              <input className="input" type="text" placeholder="5511999999999"
                value={testeFone} onChange={e => setTesteFone(e.target.value.replace(/\D/g, ''))} />
            </div>
            <div className="form-group">
              <label>Mensagem</label>
              <textarea className="input" rows={3} value={testeMsg}
                onChange={e => setTesteMsg(e.target.value)} />
            </div>
            <button className="btn btn-primary" onClick={enviarTeste}
              disabled={enviando || !conectado}
              title={!conectado ? 'Conecte o WhatsApp primeiro' : undefined}>
              {enviando ? 'Enviando...' : '📤 Enviar Teste'}
            </button>
            {!conectado && (
              <p style={{ marginTop: 8, fontSize: '0.8rem', color: '#d97706' }}>
                ⚠ Conecte o WhatsApp primeiro.
              </p>
            )}
          </div>

          <div style={{
            marginTop: '1.5rem', background: '#f0f9ff', border: '1px solid #bae6fd',
            borderRadius: 8, padding: '1rem', fontSize: '0.82rem', color: '#0369a1', lineHeight: 1.7,
          }}>
            <strong>Info técnica</strong><br />
            A Evolution API roda como container no Aspire. O download da imagem ocorre automaticamente na primeira inicialização.<br />
            Painel: <strong>http://localhost:8080</strong> · API Key: <code>jiujitsu-dev-key</code>
          </div>
        </div>
      )}
    </div>
  )
}
