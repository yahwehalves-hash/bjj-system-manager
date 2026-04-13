import { useEffect, useState } from 'react'
import { mensalidadesApi } from '../api/mensalidadesApi'
import { DatePicker, MonthPicker } from '../components/DatePicker'

const STATUS_CORES = {
  Pendente:     'badge-warning',
  Paga:         'badge-success',
  Vencida:      'badge-danger',
  Inadimplente: 'badge-danger',
  Negociada:    'badge-info',
  Cancelada:    'badge-inactive',
}

const FORMAS_PAGAMENTO = ['Dinheiro', 'Pix', 'Cartao', 'Boleto']
const FORMAS_ENUM = { Dinheiro: 1, Pix: 2, Cartao: 3, Boleto: 4 }

export default function MensalidadesPage({ usuario }) {
  const isAdmin = usuario?.role === 'Admin'

  const [mensalidades, setMensalidades] = useState([])
  const [total, setTotal]               = useState(0)
  const [pagina, setPagina]             = useState(1)
  const [filtros, setFiltros]           = useState({ status: '', competencia: '' })
  const [alerta, setAlerta]             = useState({ tipo: '', msg: '' })
  const [modal, setModal]               = useState(null) // { tipo: 'pagamento'|'negociacao', mensalidade }
  const [formModal, setFormModal]       = useState({})
  const [modalGerar, setModalGerar]     = useState(false)
  const [competenciaGerar, setCompetenciaGerar] = useState('')
  const [gerando, setGerando]           = useState(false)
  const [atualizandoStatus, setAtualizandoStatus] = useState(false)
  const tamanhoPagina = 15

  useEffect(() => { carregar() }, [pagina, filtros])

  async function carregar() {
    try {
      const res = await mensalidadesApi.listar({
        status:       filtros.status || undefined,
        competencia:  filtros.competencia || undefined,
        pagina,
        tamanhoPagina,
      })
      setMensalidades(res.data.itens)
      setTotal(res.data.totalItens)
    } catch {
      mostrarAlerta('error', 'Erro ao carregar mensalidades.')
    }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg })
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 3000)
  }

  function abrirPagamento(m) {
    setFormModal({
      valorPago:     m.valor,
      dataPagamento: new Date().toISOString().slice(0, 10),
      formaPagamento: 'Pix',
      observacao:    '',
    })
    setModal({ tipo: 'pagamento', mensalidade: m })
  }

  function abrirNegociacao(m) {
    setFormModal({
      novoValor:         m.valor,
      novaDataVencimento: m.dataVencimento,
      observacao:        '',
    })
    setModal({ tipo: 'negociacao', mensalidade: m })
  }

  async function confirmarPagamento() {
    try {
      await mensalidadesApi.registrarPagamento(modal.mensalidade.id, {
        valorPago:      Number(formModal.valorPago),
        dataPagamento:  formModal.dataPagamento,
        formaPagamento: FORMAS_ENUM[formModal.formaPagamento],
        observacao:     formModal.observacao || null,
      })
      setModal(null)
      mostrarAlerta('success', 'Pagamento registrado com sucesso.')
      await carregar()
    } catch (e) {
      mostrarAlerta('error', e.response?.data?.erro || 'Erro ao registrar pagamento.')
    }
  }

  async function confirmarNegociacao() {
    try {
      await mensalidadesApi.negociar(modal.mensalidade.id, {
        novoValor:         Number(formModal.novoValor),
        novaDataVencimento: formModal.novaDataVencimento,
        observacao:        formModal.observacao || null,
      })
      setModal(null)
      mostrarAlerta('success', 'Negociação registrada com sucesso.')
      await carregar()
    } catch (e) {
      mostrarAlerta('error', e.response?.data?.erro || 'Erro ao negociar.')
    }
  }

  async function gerarMensalidades() {
    if (!competenciaGerar) { mostrarAlerta('error', 'Selecione a competência.'); return }
    setGerando(true)
    try {
      const competencia = competenciaGerar + '-01'
      const res = await mensalidadesApi.gerar(competencia)
      setModalGerar(false)
      setCompetenciaGerar('')
      const geradas = res.data?.mensalidadesGeradas ?? 0
      mostrarAlerta('success', `${geradas} mensalidade(s) gerada(s) com sucesso.`)
      await carregar()
    } catch (e) {
      mostrarAlerta('error', e.response?.data?.erro || 'Erro ao gerar mensalidades.')
    } finally {
      setGerando(false)
    }
  }

  async function atualizarStatus() {
    setAtualizandoStatus(true)
    try {
      const res = await mensalidadesApi.atualizarStatus()
      const atualizadas = res.data?.mensalidadesAtualizadas ?? 0
      mostrarAlerta('success', `${atualizadas} mensalidade(s) tiveram status atualizado.`)
      await carregar()
    } catch (e) {
      mostrarAlerta('error', e.response?.data?.erro || 'Erro ao atualizar status.')
    } finally {
      setAtualizandoStatus(false)
    }
  }

  const totalPaginas = Math.ceil(total / tamanhoPagina)

  return (
    <div className="page-container">
      <div className="page-header">
        <h2>Mensalidades</h2>
        {isAdmin && (
          <div style={{ display: 'flex', gap: 8 }}>
            <button className="btn btn-secondary" onClick={atualizarStatus} disabled={atualizandoStatus}>
              {atualizandoStatus ? 'Atualizando...' : 'Atualizar Status'}
            </button>
            <button className="btn btn-primary" onClick={() => setModalGerar(true)}>
              Gerar Mensalidades
            </button>
          </div>
        )}
      </div>

      {alerta.msg && <div className={`alert alert-${alerta.tipo}`}>{alerta.msg}</div>}

      <div className="filtros-bar">
        <select className="input" value={filtros.status}
          onChange={e => { setFiltros({ ...filtros, status: e.target.value }); setPagina(1) }}>
          <option value="">Todos os status</option>
          {['Pendente','Paga','Vencida','Inadimplente','Negociada','Cancelada'].map(s =>
            <option key={s} value={s}>{s}</option>)}
        </select>
        <MonthPicker value={filtros.competencia}
          onChange={v => { setFiltros({ ...filtros, competencia: v }); setPagina(1) }} />
        <button className="btn btn-secondary" onClick={() => { setFiltros({ status: '', competencia: '' }); setPagina(1) }}>
          Limpar
        </button>
      </div>

      <table className="table">
        <thead>
          <tr>
            <th>Atleta</th><th>Filial</th><th>Competência</th>
            <th>Valor</th><th>Vencimento</th><th>Status</th><th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {mensalidades.map(m => (
            <tr key={m.id}>
              <td>{m.nomeAtleta}</td>
              <td>{m.nomeFilial}</td>
              <td>{m.competencia?.slice(0, 7)}</td>
              <td>R$ {Number(m.valor).toFixed(2)}</td>
              <td>{m.dataVencimento}</td>
              <td><span className={`badge ${STATUS_CORES[m.status] || ''}`}>{m.status}</span></td>
              <td>
                {['Pendente','Vencida','Inadimplente','Negociada'].includes(m.status) && (
                  <button className="btn btn-sm btn-primary" onClick={() => abrirPagamento(m)}>Pagar</button>
                )}
                {['Vencida','Inadimplente'].includes(m.status) && (
                  <button className="btn btn-sm btn-secondary" onClick={() => abrirNegociacao(m)}>Negociar</button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="paginacao">
        <button className="btn btn-secondary" onClick={() => setPagina(p => p - 1)} disabled={pagina === 1}>Anterior</button>
        <span>Página {pagina} de {totalPaginas}</span>
        <button className="btn btn-secondary" onClick={() => setPagina(p => p + 1)} disabled={pagina >= totalPaginas}>Próximo</button>
      </div>

      {modal?.tipo === 'pagamento' && (
        <div className="modal-overlay">
          <div className="modal">
            <h3>Registrar Pagamento — {modal.mensalidade.nomeAtleta}</h3>
            <label>Valor Pago</label>
            <input type="number" className="input" value={formModal.valorPago}
              onChange={e => setFormModal({ ...formModal, valorPago: e.target.value })} />
            <label>Data do Pagamento</label>
            <DatePicker value={formModal.dataPagamento}
              onChange={v => setFormModal({ ...formModal, dataPagamento: v })} minYear={2020} />
            <label>Forma de Pagamento</label>
            <select className="input" value={formModal.formaPagamento}
              onChange={e => setFormModal({ ...formModal, formaPagamento: e.target.value })}>
              {FORMAS_PAGAMENTO.map(f => <option key={f} value={f}>{f}</option>)}
            </select>
            <label>Observação</label>
            <input className="input" value={formModal.observacao}
              onChange={e => setFormModal({ ...formModal, observacao: e.target.value })} />
            <div className="form-actions">
              <button className="btn btn-primary" onClick={confirmarPagamento}>Confirmar</button>
              <button className="btn btn-secondary" onClick={() => setModal(null)}>Cancelar</button>
            </div>
          </div>
        </div>
      )}

      {modalGerar && (
        <div className="modal-overlay">
          <div className="modal">
            <h3>Gerar Mensalidades</h3>
            <p style={{ fontSize: '0.9rem', color: '#4a5568', marginBottom: 4 }}>
              Gera mensalidades para todos os atletas ativos da competência selecionada.
              Atletas que já possuem mensalidade no mês serão ignorados.
            </p>
            <label>Competência *</label>
            <MonthPicker value={competenciaGerar} onChange={setCompetenciaGerar} required />
            <div className="form-actions">
              <button className="btn btn-primary" onClick={gerarMensalidades} disabled={gerando}>
                {gerando ? 'Gerando...' : 'Gerar'}
              </button>
              <button className="btn btn-secondary" onClick={() => { setModalGerar(false); setCompetenciaGerar('') }} disabled={gerando}>
                Cancelar
              </button>
            </div>
          </div>
        </div>
      )}

      {modal?.tipo === 'negociacao' && (
        <div className="modal-overlay">
          <div className="modal">
            <h3>Negociar — {modal.mensalidade.nomeAtleta}</h3>
            <label>Novo Valor</label>
            <input type="number" className="input" value={formModal.novoValor}
              onChange={e => setFormModal({ ...formModal, novoValor: e.target.value })} />
            <label>Nova Data de Vencimento</label>
            <DatePicker value={formModal.novaDataVencimento}
              onChange={v => setFormModal({ ...formModal, novaDataVencimento: v })} minYear={2020} maxYear={2030} />
            <label>Observação</label>
            <input className="input" value={formModal.observacao}
              onChange={e => setFormModal({ ...formModal, observacao: e.target.value })} />
            <div className="form-actions">
              <button className="btn btn-primary" onClick={confirmarNegociacao}>Confirmar</button>
              <button className="btn btn-secondary" onClick={() => setModal(null)}>Cancelar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
