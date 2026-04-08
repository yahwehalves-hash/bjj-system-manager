import { useEffect, useState } from 'react'
import { configuracoesApi } from '../api/configuracoesApi'
import { filiaisApi } from '../api/filiaisApi'

export default function ConfiguracoesPage({ usuario }) {
  const [global, setGlobal]         = useState(null)
  const [filiais, setFiliais]       = useState([])
  const [filialSel, setFilialSel]   = useState('')
  const [efetiva, setEfetiva]       = useState(null)
  const [formGlobal, setFormGlobal] = useState({})
  const [formFilial, setFormFilial] = useState({})
  const [alerta, setAlerta]         = useState({ tipo: '', msg: '' })
  const isAdmin = usuario?.role === 'Admin'

  useEffect(() => {
    if (isAdmin) {
      carregarGlobal()
      filiaisApi.listar(true).then(r => setFiliais(r.data))
    }
  }, [])

  useEffect(() => {
    if (filialSel) carregarEfetiva(filialSel)
  }, [filialSel])

  async function carregarGlobal() {
    try {
      const res = await configuracoesApi.obterGlobal()
      setGlobal(res.data)
      setFormGlobal(res.data || {})
    } catch { mostrarAlerta('error', 'Erro ao carregar configuração global.') }
  }

  async function carregarEfetiva(filialId) {
    try {
      const res = await configuracoesApi.obterEfetiva(filialId)
      setEfetiva(res.data)
      setFormFilial({
        valorMensalidadePadrao:      res.data.valorMensalidadePadrao,
        diaVencimento:               res.data.diaVencimento,
        toleranciaInadimplenciaDias: res.data.toleranciaInadimplenciaDias,
        multaAtrasoPercentual:       res.data.multaAtrasoPercentual,
        jurosDiarioPercentual:       res.data.jurosDiarioPercentual,
        descontoAntecipacaoPercentual: res.data.descontoAntecipacaoPercentual,
      })
    } catch { mostrarAlerta('error', 'Erro ao carregar configuração da filial.') }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg })
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 3000)
  }

  async function salvarGlobal() {
    try {
      await configuracoesApi.atualizarGlobal({
        valorMensalidadePadrao:      Number(formGlobal.valorMensalidadePadrao),
        diaVencimento:               Number(formGlobal.diaVencimento),
        toleranciaInadimplenciaDias: Number(formGlobal.toleranciaInadimplenciaDias),
        multaAtrasoPercentual:       Number(formGlobal.multaAtrasoPercentual),
        jurosDiarioPercentual:       Number(formGlobal.jurosDiarioPercentual),
        descontoAntecipacaoPercentual: Number(formGlobal.descontoAntecipacaoPercentual),
      })
      mostrarAlerta('success', 'Configuração global atualizada.')
      await carregarGlobal()
    } catch { mostrarAlerta('error', 'Erro ao salvar configuração global.') }
  }

  async function salvarFilial() {
    if (!filialSel) { mostrarAlerta('error', 'Selecione uma filial.'); return }
    try {
      await configuracoesApi.atualizarFilial(filialSel, {
        valorMensalidadePadrao:      formFilial.valorMensalidadePadrao ? Number(formFilial.valorMensalidadePadrao) : null,
        diaVencimento:               formFilial.diaVencimento ? Number(formFilial.diaVencimento) : null,
        toleranciaInadimplenciaDias: formFilial.toleranciaInadimplenciaDias ? Number(formFilial.toleranciaInadimplenciaDias) : null,
        multaAtrasoPercentual:       formFilial.multaAtrasoPercentual ? Number(formFilial.multaAtrasoPercentual) : null,
        jurosDiarioPercentual:       formFilial.jurosDiarioPercentual ? Number(formFilial.jurosDiarioPercentual) : null,
        descontoAntecipacaoPercentual: formFilial.descontoAntecipacaoPercentual ? Number(formFilial.descontoAntecipacaoPercentual) : null,
      })
      mostrarAlerta('success', 'Configuração da filial atualizada.')
    } catch { mostrarAlerta('error', 'Erro ao salvar configuração da filial.') }
  }

  const campo = (label, key, form, setForm, type = 'number') => (
    <div key={key}>
      <label>{label}</label>
      <input type={type} className="input" value={form[key] ?? ''}
        onChange={e => setForm({ ...form, [key]: e.target.value })} />
    </div>
  )

  return (
    <div className="page-container">
      <h2>Configurações</h2>
      {alerta.msg && <div className={`alert alert-${alerta.tipo}`}>{alerta.msg}</div>}

      {isAdmin && (
        <>
          <div className="form-card">
            <h3>Configuração Global</h3>
            <p className="hint">Valores padrão para toda a rede. Filiais podem sobrescrever individualmente.</p>
            <div className="form-grid">
              {campo('Valor Mensalidade Padrão (R$)',  'valorMensalidadePadrao',        formGlobal, setFormGlobal)}
              {campo('Dia de Vencimento (1-28)',        'diaVencimento',                 formGlobal, setFormGlobal)}
              {campo('Tolerância Inadimplência (dias)', 'toleranciaInadimplenciaDias',   formGlobal, setFormGlobal)}
              {campo('Multa Atraso (%)',                'multaAtrasoPercentual',          formGlobal, setFormGlobal)}
              {campo('Juros Diário (%)',                'jurosDiarioPercentual',          formGlobal, setFormGlobal)}
              {campo('Desconto Antecipação (%)',        'descontoAntecipacaoPercentual',  formGlobal, setFormGlobal)}
            </div>
            <button className="btn btn-primary" onClick={salvarGlobal}>Salvar Global</button>
          </div>

          <div className="form-card">
            <h3>Configuração por Filial</h3>
            <p className="hint">Deixe em branco para herdar o valor global.</p>
            <select className="input" value={filialSel}
              onChange={e => setFilialSel(e.target.value)}>
              <option value="">Selecione a filial</option>
              {filiais.map(f => <option key={f.id} value={f.id}>{f.nome}</option>)}
            </select>
            {efetiva && (
              <>
                <div className="form-grid" style={{ marginTop: '1rem' }}>
                  {campo('Valor Mensalidade (R$)',        'valorMensalidadePadrao',        formFilial, setFormFilial)}
                  {campo('Dia de Vencimento',              'diaVencimento',                 formFilial, setFormFilial)}
                  {campo('Tolerância Inadimplência (dias)','toleranciaInadimplenciaDias',   formFilial, setFormFilial)}
                  {campo('Multa Atraso (%)',               'multaAtrasoPercentual',          formFilial, setFormFilial)}
                  {campo('Juros Diário (%)',               'jurosDiarioPercentual',          formFilial, setFormFilial)}
                  {campo('Desconto Antecipação (%)',       'descontoAntecipacaoPercentual',  formFilial, setFormFilial)}
                </div>
                <button className="btn btn-primary" onClick={salvarFilial}>Salvar Filial</button>
              </>
            )}
          </div>
        </>
      )}
    </div>
  )
}
