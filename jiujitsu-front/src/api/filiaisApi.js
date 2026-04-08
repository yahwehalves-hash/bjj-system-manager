import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const filiaisApi = {
  listar:    (ativo)   => api.get('/api/filiais', { params: { ativo } }),
  obterPorId: (id)     => api.get(`/api/filiais/${id}`),
  criar:     (dados)   => api.post('/api/filiais', dados),
  atualizar: (id, dados) => api.put(`/api/filiais/${id}`, dados),
  desativar: (id)      => api.delete(`/api/filiais/${id}`),
}
