import { useEffect, useState } from 'react'
import { dashboardApi } from '../api/dashboardApi'

const competenciaAtual = () => new Date().toISOString().slice(0, 7)

export default function DashboardPage({ usuario }) {
  const [dados, setDados]           = useState(null)
  const [consolidado, setConsolidado] = useState(null)
  const [competencia, setCompetencia] = useState(competenciaAtual())
  const [erro, setErro]             = useState('')

  const isAdmin = usuario?.role === 'Admin'

  useEffect(() => { carregar() }, [competencia])

  async function carregar() {
    setErro('')
    try {
      if (isAdmin) {
        const [dashRes, consRes] = await Promise.all([
          dashboardApi.obterConsolidado({ competencia }),
          dashboardApi.obterConsolidado({ competencia }),
        ])
        setConsolidado(dashRes.data)
      } else {
        const res = await dashboardApi.obterFinanceiro({ competencia })
        setDados(res.data)
      }
    } catch {
      setErro('Erro ao carregar dados do dashboard.')
    }
  }

  const formatMoeda = (v) => `R$ ${Number(v || 0).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`
  const formatPct   = (v) => `${Number(v || 0).toFixed(1)}%`

  const renderFilial = (f, key) => (
    <div key={key} className="dashboard-card">
      <h3>{f.nomeFilial}</h3>
      <div className="dashboard-grid">
        <div className="metric">
          <span className="metric-label">Alunos Ativos</span>
          <span className="metric-value">{f.totalAtletasAtivos}</span>
        </div>
        <div className="metric metric-danger">
          <span className="metric-label">Inadimplentes</span>
          <span className="metric-value">{f.totalInadimplentes} ({formatPct(f.percentualInadimplencia)})</span>
        </div>
        <div className="metric">
          <span className="metric-label">Receita Prevista</span>
          <span className="metric-value">{formatMoeda(f.receitaPrevista)}</span>
        </div>
        <div className="metric metric-success">
          <span className="metric-label">Receita Realizada</span>
          <span className="metric-value">{formatMoeda(f.receitaRealizada)}</span>
        </div>
        <div className="metric metric-warning">
          <span className="metric-label">Total Despesas</span>
          <span className="metric-value">{formatMoeda(f.totalDespesas)}</span>
        </div>
        <div className={`metric ${f.resultadoOperacional >= 0 ? 'metric-success' : 'metric-danger'}`}>
          <span className="metric-label">Resultado</span>
          <span className="metric-value">{formatMoeda(f.resultadoOperacional)}</span>
        </div>
        <div className="metric metric-info">
          <span className="metric-label">Pendentes</span>
          <span className="metric-value">{f.mensalidadesPendentes}</span>
        </div>
        <div className="metric metric-warning">
          <span className="metric-label">Vencidas</span>
          <span className="metric-value">{f.mensalidadesVencidas}</span>
        </div>
      </div>
    </div>
  )

  return (
    <div className="page-container">
      <div className="page-header">
        <h2>Dashboard Financeiro</h2>
        <input
          type="month"
          value={competencia}
          onChange={e => setCompetencia(e.target.value)}
          className="input"
        />
      </div>

      {erro && <div className="alert alert-error">{erro}</div>}

      {isAdmin && consolidado && (
        <>
          <div className="dashboard-card consolidado">
            <h3>Consolidado — Toda a Rede</h3>
            <div className="dashboard-grid">
              <div className="metric">
                <span className="metric-label">Total Alunos Ativos</span>
                <span className="metric-value">{consolidado.totalAtletasAtivos}</span>
              </div>
              <div className="metric metric-success">
                <span className="metric-label">Receita Total</span>
                <span className="metric-value">{formatMoeda(consolidado.totalReceitaRealizada)}</span>
              </div>
              <div className="metric metric-warning">
                <span className="metric-label">Despesas Totais</span>
                <span className="metric-value">{formatMoeda(consolidado.totalDespesas)}</span>
              </div>
              <div className={`metric ${consolidado.resultadoConsolidado >= 0 ? 'metric-success' : 'metric-danger'}`}>
                <span className="metric-label">Resultado Consolidado</span>
                <span className="metric-value">{formatMoeda(consolidado.resultadoConsolidado)}</span>
              </div>
            </div>
          </div>
          <h3 style={{ marginTop: '1.5rem' }}>Por Filial</h3>
          {consolidado.filiais?.map((f, i) => renderFilial(f, i))}
        </>
      )}

      {!isAdmin && dados && renderFilial(dados, 'minha-filial')}
    </div>
  )
}
