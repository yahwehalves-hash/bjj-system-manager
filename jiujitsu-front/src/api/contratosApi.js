import axios from 'axios'

const BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000'

const api = axios.create({ baseURL: BASE })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const contratosApi = {
  status: (atletaId) =>
    api.get(`/api/contratos/atletas/${atletaId}/status`).then(r => r.data),

  aceitar: (atletaId) =>
    api.post(`/api/contratos/atletas/${atletaId}/aceitar`).then(r => r.data),

  async downloadPdf(atletaId) {
    const token = localStorage.getItem('token')
    const res = await fetch(`${BASE}/api/contratos/atletas/${atletaId}/pdf`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (!res.ok) throw new Error('Erro ao gerar PDF')
    const blob = await res.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `contrato_${atletaId}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  },

  salvarTemplate: (filialId, conteudo) =>
    api.put('/api/contratos/template', { filialId: filialId || null, conteudo }).then(r => r.data),
}
