import { useEffect, useState } from 'react'
import { filiaisApi } from '../api/filiaisApi'

export default function FiliaisPage() {
  const [filiais, setFiliais]     = useState([])
  const [form, setForm]           = useState({ nome: '', endereco: '', cnpj: '', telefone: '' })
  const [editandoId, setEditandoId] = useState(null)
  const [alerta, setAlerta]       = useState({ tipo: '', msg: '' })
  const [loading, setLoading]     = useState(false)

  useEffect(() => { carregar() }, [])

  async function carregar() {
    try {
      const res = await filiaisApi.listar()
      setFiliais(res.data)
    } catch {
      mostrarAlerta('error', 'Erro ao carregar filiais.')
    }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg })
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 3000)
  }

  function iniciarEdicao(f) {
    setEditandoId(f.id)
    setForm({ nome: f.nome, endereco: f.endereco || '', cnpj: f.cnpj || '', telefone: f.telefone || '' })
  }

  function cancelarEdicao() {
    setEditandoId(null)
    setForm({ nome: '', endereco: '', cnpj: '', telefone: '' })
  }

  async function salvar() {
    if (!form.nome.trim()) { mostrarAlerta('error', 'Nome é obrigatório.'); return }
    setLoading(true)
    try {
      if (editandoId) {
        await filiaisApi.atualizar(editandoId, form)
        mostrarAlerta('success', 'Filial atualizada com sucesso.')
      } else {
        await filiaisApi.criar(form)
        mostrarAlerta('success', 'Filial criada com sucesso.')
      }
      cancelarEdicao()
      await carregar()
    } catch (e) {
      mostrarAlerta('error', e.response?.data?.erro || 'Erro ao salvar.')
    } finally {
      setLoading(false)
    }
  }

  async function desativar(id) {
    if (!confirm('Desativar esta filial?')) return
    try {
      await filiaisApi.desativar(id)
      mostrarAlerta('success', 'Filial desativada.')
      await carregar()
    } catch {
      mostrarAlerta('error', 'Erro ao desativar filial.')
    }
  }

  return (
    <div className="page-container">
      <h2>Gerenciar Filiais</h2>

      {alerta.msg && <div className={`alert alert-${alerta.tipo}`}>{alerta.msg}</div>}

      <div className="form-card">
        <h3>{editandoId ? 'Editar Filial' : 'Nova Filial'}</h3>
        <div className="form-row">
          <input className="input" placeholder="Nome *" value={form.nome}
            onChange={e => setForm({ ...form, nome: e.target.value })} />
          <input className="input" placeholder="CNPJ" value={form.cnpj}
            onChange={e => setForm({ ...form, cnpj: e.target.value })} />
        </div>
        <div className="form-row">
          <input className="input" placeholder="Endereço" value={form.endereco}
            onChange={e => setForm({ ...form, endereco: e.target.value })} />
          <input className="input" placeholder="Telefone" value={form.telefone}
            onChange={e => setForm({ ...form, telefone: e.target.value })} />
        </div>
        <div className="form-actions">
          <button className="btn btn-primary" onClick={salvar} disabled={loading}>
            {loading ? 'Salvando...' : (editandoId ? 'Atualizar' : 'Criar')}
          </button>
          {editandoId && <button className="btn btn-secondary" onClick={cancelarEdicao}>Cancelar</button>}
        </div>
      </div>

      <table className="table">
        <thead>
          <tr><th>Nome</th><th>CNPJ</th><th>Telefone</th><th>Status</th><th>Ações</th></tr>
        </thead>
        <tbody>
          {filiais.map(f => (
            <tr key={f.id}>
              <td>{f.nome}</td>
              <td>{f.cnpj || '—'}</td>
              <td>{f.telefone || '—'}</td>
              <td><span className={`badge ${f.ativo ? 'badge-success' : 'badge-inactive'}`}>
                {f.ativo ? 'Ativa' : 'Inativa'}
              </span></td>
              <td>
                <button className="btn btn-sm btn-secondary" onClick={() => iniciarEdicao(f)}>Editar</button>
                {f.ativo && <button className="btn btn-sm btn-danger" onClick={() => desativar(f.id)}>Desativar</button>}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
