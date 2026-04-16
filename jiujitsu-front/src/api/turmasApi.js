import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const turmasApi = {
  listar:           (params) => api.get('/api/turmas', { params }),
  obterPorId:       (id)     => api.get(`/api/turmas/${id}`),
  criar:            (data)   => api.post('/api/turmas', data),
  atualizar:        (id, data) => api.put(`/api/turmas/${id}`, data),
  desativar:        (id)     => api.delete(`/api/turmas/${id}`),
  vincularAtleta:   (id, atletaId) => api.post(`/api/turmas/${id}/atletas/${atletaId}`),
  desvincularAtleta:(id, atletaId) => api.delete(`/api/turmas/${id}/atletas/${atletaId}`),
}
