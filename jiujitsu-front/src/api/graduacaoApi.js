import axios from 'axios'

const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export const graduacaoApi = {
  listarRegras:    (params) => api.get('/api/graduacao/regras', { params }),
  salvarRegra:     (data)   => api.put('/api/graduacao/regras', data),
  listarElegiveis: (params) => api.get('/api/graduacao/elegiveis', { params }),
}
