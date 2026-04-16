import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const planosApi = {
  listar:    (filialId) => api.get('/api/planos', { params: filialId ? { filialId } : {} }).then(r => r.data),
  criar:     (dados)    => api.post('/api/planos', dados).then(r => r.data),
  atualizar: (id, dados) => api.put(`/api/planos/${id}`, dados).then(r => r.data),
  desativar: (id)       => api.delete(`/api/planos/${id}`).then(r => r.data),
}

export const matriculasApi = {
  listar:   (atletaId) => api.get('/api/matriculas', { params: atletaId ? { atletaId } : {} }).then(r => r.data),
  criar:    (dados)    => api.post('/api/matriculas', dados).then(r => r.data),
  cancelar: (id, motivo) => api.delete(`/api/matriculas/${id}`, { data: { motivo } }).then(r => r.data),
}
