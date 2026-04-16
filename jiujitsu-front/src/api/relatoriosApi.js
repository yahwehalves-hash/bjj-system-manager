import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

function downloadBlob(blob, filename) {
  const url = URL.createObjectURL(blob)
  const a   = document.createElement('a')
  a.href     = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

export const relatoriosApi = {
  async inadimplencia({ competencia, filialId } = {}) {
    const res = await api.get('/api/relatorios/inadimplencia', {
      params:       { competencia, filialId },
      responseType: 'blob',
    })
    downloadBlob(res.data, `inadimplencia_${competencia || 'mes'}.xlsx`)
  },

  async dre({ competencia, filialId } = {}) {
    const res = await api.get('/api/relatorios/dre', {
      params:       { competencia, filialId },
      responseType: 'blob',
    })
    downloadBlob(res.data, `dre_${competencia || 'mes'}.xlsx`)
  },

  async atletasPorFaixa({ filialId } = {}) {
    const res = await api.get('/api/relatorios/atletas-por-faixa', {
      params:       { filialId },
      responseType: 'blob',
    })
    downloadBlob(res.data, `atletas_por_faixa.xlsx`)
  },
}
