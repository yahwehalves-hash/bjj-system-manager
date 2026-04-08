import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const mensalidadesApi = {
  listar:            (params) => api.get('/api/mensalidades', { params }),
  obterPorId:        (id)     => api.get(`/api/mensalidades/${id}`),
  registrarPagamento: (id, dados) => api.post(`/api/mensalidades/${id}/pagamento`, dados),
  negociar:          (id, dados) => api.post(`/api/mensalidades/${id}/negociacao`, dados),
  cancelar:          (id, motivo) => api.delete(`/api/mensalidades/${id}`, { data: { motivo } }),
  gerar:             (competencia) => api.post('/api/mensalidades/gerar', { competencia }),
}
