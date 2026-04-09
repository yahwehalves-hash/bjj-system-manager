import { useEffect, useState } from 'react'
import { despesasApi } from '../api/despesasApi'
import { filiaisApi } from '../api/filiaisApi'
import { DatePicker, MonthPicker } from '../components/DatePicker'

const CATEGORIAS = ['Pessoal', 'Infraestrutura', 'Material', 'Administrativo']
const FORMAS = ['Dinheiro', 'Pix', 'Cartao', 'Boleto']

// Mapeamento para os valores numéricos dos enums no backend
const CATEGORIAS_ENUM = { Pessoal: 1, Infraestrutura: 2, Material: 3, Administrativo: 4 }
const FORMAS_ENUM     = { Dinheiro: 1, Pix: 2, Cartao: 3, Boleto: 4 }

const STATUS_CORES = { APagar: 'badge-warning', Paga: 'badge-success', Cancelada: 'badge-inactive' }

export default function DespesasPage({ usuario }) {
  const [despesas, setDespesas]   = useState([])
  const [total, setTotal]         = useState(0)
  const [pagina, setPagina]       = useState(1)
  const [filiais, setFiliais]     = useState([])
  const [filtros, setFiltros]     = useState({ categoria: '', status: '' })
  const [alerta, setAlerta]       = useState({ tipo: '', msg: '' })
  const [modal, setModal]         = useState(null)
  const [modalPagar, setModalPagar] = useState(null) // { despesaId }
  const [formPagar, setFormPagar] = useState({ dataPagamento: '', formaPagamento: 'Pix' })
  const [form, setForm]           = useState({
    filialId: '', descricao: '', categoria: 'Pessoal', subcategoria: '',
    valor: '', dataCompetencia: '', dataPagamento: '', formaPagamento: '', observacao: '',
  })
  const tamanhoPagina = 15
  const isAdmin = usuario?.role === 'Admin'

  useEffect(() => {
    carregar()
    if (isAdmin) filiaisApi.listar(true).then(r => setFiliais(r.data))
  }, [pagina, filtros])

  async function carregar() {
    try {
      const res = await despesasApi.listar({
        categoria:    filtros.categoria || undefined,
        status:       filtros.status    || undefined,
        pagina,
        tamanhoPagina,
      })
      setDespesas(res.data.itens)
      setTotal(res.data.totalItens)
    } catch {
      mostrarAlerta('error', 'Erro ao carregar despesas.')
    }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg })
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 3000)
  }

  async function salvar() {
    if (!form.descricao || !form.subcategoria || !form.valor || !form.dataCompetencia) {
      mostrarAlerta('error', 'Preencha todos os campos obrigatórios.')
      return
    }
    try {
      await despesasApi.lancar({
        filialId:        form.filialId || undefined,
        descricao:       form.descricao,
        categoria:       CATEGORIAS_ENUM[form.categoria],
        subcategoria:    form.subcategoria,
        valor:           Number(form.valor),
        dataCompetencia: form.dataCompetencia,
        dataPagamento:   form.dataPagamento || null,
        formaPagamento:  form.formaPagamento ? FORMAS_ENUM[form.formaPagamento] : null,
        observacao:      form.observacao    || null,
      })
      setModal(null)
      mostrarAlerta('success', 'Despesa lançada com sucesso.')
      await carregar()
    } catch (e) {
      mostrarAlerta('error', e.response?.data?.erro || 'Erro ao lançar despesa.')
    }
  }

  function abrirPagar(id) {
    setFormPagar({ dataPagamento: new Date().toISOString().slice(0, 10), formaPagamento: 'Pix' })
    setModalPagar({ despesaId: id })
  }

  async function confirmarPagamento() {
    if (!formPagar.dataPagamento) { mostrarAlerta('error', 'Informe a data de pagamento.'); return }
    try {
      await despesasApi.marcarComoPaga(modalPagar.despesaId, {
        dataPagamento:  formPagar.dataPagamento,
        formaPagamento: FORMAS_ENUM[formPagar.formaPagamento],
        observacao:     null,
      })
      setModalPagar(null)
      mostrarAlerta('success', 'Despesa marcada como paga.')
      await carregar()
    } catch { mostrarAlerta('error', 'Erro ao marcar como paga.') }
  }

  async function cancelar(id) {
    if (!confirm('Cancelar esta despesa?')) return
    try {
      await despesasApi.cancelar(id, null)
      mostrarAlerta('success', 'Despesa cancelada.')
      await carregar()
    } catch { mostrarAlerta('error', 'Erro ao cancelar.') }
  }

  const totalPaginas = Math.ceil(total / tamanhoPagina)

  return (
    <div className="page-container">
      <div className="page-header">
        <h2>Despesas</h2>
        <button className="btn btn-primary" onClick={() => setModal('nova')}>+ Nova Despesa</button>
      </div>

      {alerta.msg && <div className={`alert alert-${alerta.tipo}`}>{alerta.msg}</div>}

      <div className="filtros-bar">
        <select className="input" value={filtros.categoria}
          onChange={e => { setFiltros({ ...filtros, categoria: e.target.value }); setPagina(1) }}>
          <option value="">Todas as categorias</option>
          {CATEGORIAS.map(c => <option key={c} value={c}>{c}</option>)}
        </select>
        <select className="input" value={filtros.status}
          onChange={e => { setFiltros({ ...filtros, status: e.target.value }); setPagina(1) }}>
          <option value="">Todos os status</option>
          {['APagar','Paga','Cancelada'].map(s => <option key={s} value={s}>{s}</option>)}
        </select>
        <button className="btn btn-secondary" onClick={() => { setFiltros({ categoria: '', status: '' }); setPagina(1) }}>
          Limpar
        </button>
      </div>

      <table className="table">
        <thead>
          <tr><th>Filial</th><th>Descrição</th><th>Categoria</th><th>Competência</th><th>Valor</th><th>Status</th><th>Ações</th></tr>
        </thead>
        <tbody>
          {despesas.map(d => (
            <tr key={d.id}>
              <td>{d.nomeFilial}</td>
              <td>{d.descricao}</td>
              <td>{d.categoria} / {d.subcategoria}</td>
              <td>{d.dataCompetencia}</td>
              <td>R$ {Number(d.valor).toFixed(2)}</td>
              <td><span className={`badge ${STATUS_CORES[d.status] || ''}`}>{d.status}</span></td>
              <td>
                {d.status === 'APagar' && (
                  <button className="btn btn-sm btn-primary" onClick={() => abrirPagar(d.id)}>Pagar</button>
                )}
                {d.status !== 'Cancelada' && (
                  <button className="btn btn-sm btn-danger" onClick={() => cancelar(d.id)}>Cancelar</button>
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

      {/* Modal: Nova Despesa */}
      {modal === 'nova' && (
        <div className="modal-overlay">
          <div className="modal">
            <h3>Nova Despesa</h3>
            {isAdmin && (
              <>
                <label>Filial</label>
                <select className="input" value={form.filialId}
                  onChange={e => setForm({ ...form, filialId: e.target.value })}>
                  <option value="">Selecione a filial</option>
                  {filiais.map(f => <option key={f.id} value={f.id}>{f.nome}</option>)}
                </select>
              </>
            )}
            <label>Descrição *</label>
            <input className="input" value={form.descricao}
              onChange={e => setForm({ ...form, descricao: e.target.value })} />
            <div className="form-row">
              <div>
                <label>Categoria *</label>
                <select className="input" value={form.categoria}
                  onChange={e => setForm({ ...form, categoria: e.target.value })}>
                  {CATEGORIAS.map(c => <option key={c} value={c}>{c}</option>)}
                </select>
              </div>
              <div>
                <label>Subcategoria *</label>
                <input className="input" value={form.subcategoria}
                  onChange={e => setForm({ ...form, subcategoria: e.target.value })} />
              </div>
            </div>
            <div className="form-row">
              <div>
                <label>Valor *</label>
                <input type="number" className="input" value={form.valor}
                  onChange={e => setForm({ ...form, valor: e.target.value })} />
              </div>
              <div>
                <label>Competência *</label>
                <MonthPicker
                  value={form.dataCompetencia ? form.dataCompetencia.slice(0, 7) : ''}
                  onChange={v => setForm({ ...form, dataCompetencia: v ? v + '-01' : '' })}
                />
              </div>
            </div>
            <div className="form-row">
              <div>
                <label>Data Pagamento</label>
                <DatePicker value={form.dataPagamento}
                  onChange={v => setForm({ ...form, dataPagamento: v })} minYear={2020} />
              </div>
              <div>
                <label>Forma Pagamento</label>
                <select className="input" value={form.formaPagamento}
                  onChange={e => setForm({ ...form, formaPagamento: e.target.value })}>
                  <option value="">—</option>
                  {FORMAS.map(f => <option key={f} value={f}>{f}</option>)}
                </select>
              </div>
            </div>
            <label>Observação</label>
            <input className="input" value={form.observacao}
              onChange={e => setForm({ ...form, observacao: e.target.value })} />
            <div className="form-actions">
              <button className="btn btn-primary" onClick={salvar}>Salvar</button>
              <button className="btn btn-secondary" onClick={() => setModal(null)}>Cancelar</button>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Pagar Despesa */}
      {modalPagar && (
        <div className="modal-overlay">
          <div className="modal">
            <h3>Registrar Pagamento</h3>
            <label>Data de Pagamento *</label>
            <DatePicker value={formPagar.dataPagamento}
              onChange={v => setFormPagar({ ...formPagar, dataPagamento: v })} minYear={2020} />
            <label>Forma de Pagamento</label>
            <select className="input" value={formPagar.formaPagamento}
              onChange={e => setFormPagar({ ...formPagar, formaPagamento: e.target.value })}>
              {FORMAS.map(f => <option key={f} value={f}>{f}</option>)}
            </select>
            <div className="form-actions">
              <button className="btn btn-primary" onClick={confirmarPagamento}>Confirmar</button>
              <button className="btn btn-secondary" onClick={() => setModalPagar(null)}>Cancelar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
