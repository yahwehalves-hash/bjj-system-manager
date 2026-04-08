import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const despesasApi = {
  listar:        (params)      => api.get('/api/despesas', { params }),
  obterPorId:    (id)          => api.get(`/api/despesas/${id}`),
  lancar:        (dados)       => api.post('/api/despesas', dados),
  marcarComoPaga: (id, dados)  => api.put(`/api/despesas/${id}/pagamento`, dados),
  cancelar:      (id, motivo)  => api.delete(`/api/despesas/${id}`, { data: { motivo } }),
}
